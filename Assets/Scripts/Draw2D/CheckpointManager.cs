using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Prefabs")]
    public GameObject checkpointPrefab;

    public DrawingTool DrawingTool;
    public PenManager penManager;
    public UndoRedoManager undoRedoManager;
    public StoragePermissionRequester permissionRequester;

    public LineType currentLineType = LineType.Wall;
    public List<WallLine> wallLines = new List<WallLine>();
    public List<Room> rooms = new List<Room>();

    [Header("Camera")]
    public Camera drawingCamera; // Gán Camera chính vẽ 2D

    public bool isMovingCheckpoint = false;
    public List<GameObject> currentCheckpoints = new List<GameObject>();
    public GameObject selectedCheckpoint = null; // Điểm được chọn để di chuyển  
    private GameObject selectedExtraCheckpoint = null; // Điểm phụ được chọn để nối
    private GameObject selectedNormalCheckpoint = null; // Điểm chính được chọn để nối
    public bool isDragging = false; // Kiểm tra xem có đang kéo điểm không 
    public bool isPreviewing = false; // Trạng thái preview
    public bool isClosedLoop = false; // Biến kiểm tra xem mạch đã khép kín chưa 

    public bool IsDraggingRoom = false;
    public GameObject previewCheckpoint = null;

    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();

    public List<List<GameObject>> AllCheckpoints =>
        allCheckpoints; // Truy cập danh sách tất cả các checkpoint từ bên ngoài

    public Dictionary<string, List<GameObject>> placedPointsByRoom = new();
    public Dictionary<string, GameObject> RoomFloorMap = new(); // roomID → floor GameObject


    private float closeThreshold = 0.5f; // Khoảng cách tối đa để chọn điểm
    private Vector3 previewPosition; // Vị trí preview

    private WallLine selectedWallLineForDoor; // đoạn tường được chọn
    private Room selectedRoomForDoor;

    private Vector3? firstDoorPoint = null; // lưu P1

    // Map loop checkpoint list => Room ID
    private List<LoopMap> loopMappings = new List<LoopMap>();
    // Lưu lại tất cả các cửa / cửa sổ để chèn lại sau khi rebuild wallLines

    // [RoomID] → List<(WallLine, GameObject p1, GameObject p2)>
    public Dictionary<string, List<(WallLine line, GameObject p1, GameObject p2)>> tempDoorWindowPoints
        = new Dictionary<string, List<(WallLine, GameObject, GameObject)>>();
    // Dictionary<string (roomID), List<(WallLine, GameObject, GameObject)>> tempDoorWindowPoints;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadPointsFromRoomStorage();
    }

    void Update()
    {
        // === Pen ON ===
        if (EventSystem.current.IsPointerOverGameObject())
        {
            isPreviewing = false;
            DrawingTool.ClearPreviewLine();
            if (previewCheckpoint != null)
            {
                Destroy(previewCheckpoint);
                previewCheckpoint = null;
            }

            return;
        }

        if (Input.GetMouseButton(0))
        {
            isPreviewing = true;
            previewPosition = GetWorldPositionFromScreen(Input.mousePosition);

            if (previewCheckpoint == null)
            {
                previewCheckpoint = Instantiate(checkpointPrefab, previewPosition, Quaternion.identity);
                previewCheckpoint.name = "PreviewCheckpoint";
            }

            previewCheckpoint.transform.position = previewPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPreviewing = false;
            DrawingTool.ClearPreviewLine();

            if (previewCheckpoint != null)
            {
                Destroy(previewCheckpoint);
            }

            // HandleCheckpointPlacement(previewPosition); // Vẽ checkpoint mới
            if (currentLineType == LineType.Wall)
                HandleWallLoopPlacement(previewPosition);
            else
                HandleCheckpointPlacement(previewPosition);

            DeselectCheckpoint(); // Không thật sự cần, nhưng không hại
            isDragging = false;
        }
    }

    public void SelectCheckpoint()
    {
        Vector3 clickPosition = GetWorldPositionFromScreen(Input.mousePosition);
        TrySelectCheckpoint(clickPosition);
    }

    public void HandleCheckpointPlacement(Vector3 position)
    {
        if (selectedCheckpoint != null) return; // Nếu đã chọn điểm, không cần đặt mới

        // === Nếu đang định thêm Door/Window nhưng chưa có loop kín ===
        // if ((currentLineType == LineType.Door || currentLineType == LineType.Window) && currentCheckpoints.Count > 0)
        // {
        //     ShowIncompleteLoopPopup(); // hỏi xóa hay tiếp tục vẽ để khép
        //     return;
        // }

        // === Nếu là cửa sổ/cửa và không đang vẽ loop thì xử lý riêng ===
        if (currentLineType == LineType.Door || currentLineType == LineType.Window)
        {
            InsertDoorOrWindow(position, currentLineType);
            return;
        }
    }

    public void HandleWallLoopPlacement(Vector3 position)
    {
        if (selectedCheckpoint != null) return;

        if (RoomStorage.rooms.Count == 0)
        {
            Debug.LogWarning("RoomStorage chưa sẵn sàng.");
            return;
        }

        string roomID = FindRoomIDByPoint(position);
        Debug.Log($"[HandleCheckpointPlacement] roomID tại vị trí {position} là: {roomID}");

        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogWarning("Không nằm trong phòng nào --> không đặt checkpoint.");
            return;
        }

        // === Tạo checkpoint sau khi đảm bảo nằm trong room ===
        GameObject checkpoint = Instantiate(checkpointPrefab, position, Quaternion.identity);
        currentCheckpoints.Add(checkpoint);

        if (RoomFloorMap.TryGetValue(roomID, out GameObject parentFloor))
        {
            checkpoint.transform.SetParent(parentFloor.transform);
            checkpoint.tag = "CheckpointExtra";

            Room existingRoom = RoomStorage.GetRoomByID(roomID);
            if (existingRoom != null)
            {
                existingRoom.extraCheckpoints.Add(new Vector2(position.x, position.z));
                RoomStorage.UpdateOrAddRoom(existingRoom);
                Debug.Log($"Đã thêm checkpoint phụ vào Room {roomID}");
            }
        }
    }

    private string FindRoomIDByPoint(Vector3 worldPos)
    {
        Vector2 point2D = new Vector2(worldPos.x, worldPos.z);
        foreach (Room room in RoomStorage.rooms)
        {
            if (IsPointInPolygon(point2D, room.checkpoints))
            {
                return room.ID;
            }
        }

        return null;
    }

    // Hàm kiểm tra điểm có nằm trong polygon (ray casting algorithm)
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int j = polygon.Count - 1;
        bool inside = false;

        for (int i = 0; i < polygon.Count; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public void InsertDoorOrWindow(Vector3 clickPosition, LineType type)
    {
        if (firstDoorPoint == null)
        {
            // Lần click đầu tiên: chọn đoạn wall gần nhất
            float minDist = float.MaxValue;
            selectedRoomForDoor = null;
            selectedWallLineForDoor = null;

            foreach (Room room in RoomStorage.rooms)
            {
                foreach (var wl in room.wallLines)
                {
                    if (wl.type != LineType.Wall) continue; // chỉ chọn từ tường thường

                    Vector3 projected = ProjectPointOnLineSegment(wl.start, wl.end, clickPosition);
                    float dist = Vector3.Distance(clickPosition, projected);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        selectedRoomForDoor = room;
                        selectedWallLineForDoor = wl;
                        firstDoorPoint = projected;
                    }
                }
            }

            if (selectedWallLineForDoor == null)
            {
                Debug.LogWarning("Không tìm thấy đoạn tường phù hợp.");
                firstDoorPoint = null;
                return;
            }

            // Hiển thị preview
            GameObject p1Obj = Instantiate(checkpointPrefab, firstDoorPoint.Value, Quaternion.identity);
            p1Obj.name = $"{type}_P1_PREVIEW";

            Debug.Log($"[InsertDoorOrWindow] Đã chọn P1: {firstDoorPoint}");
            return;
        }
        else
        {
            // Lần click thứ 2: xác định đoạn cửa
            Vector3 p1 = firstDoorPoint.Value;
            Vector3 p2 = ProjectPointOnLineSegment(selectedWallLineForDoor.start, selectedWallLineForDoor.end,
                clickPosition);

            if (Vector3.Distance(p1, p2) < 0.01f)
            {
                Debug.LogWarning("P2 trùng P1, không hợp lệ.");
                return;
            }

            // Xoá P1_PREVIEW nếu còn
            var preview = GameObject.Find($"{type}_P1_PREVIEW");
            if (preview != null) Destroy(preview);

            // Tạo WallLine mới cho cửa/cửa sổ
            WallLine door = new WallLine(p1, p2, type);
            selectedRoomForDoor.wallLines.Add(door);

            GameObject p1Obj = Instantiate(checkpointPrefab, p1, Quaternion.identity);
            GameObject p2Obj = Instantiate(checkpointPrefab, p2, Quaternion.identity);
            p1Obj.name = $"{type}_P1";
            p2Obj.name = $"{type}_P2";

            if (!tempDoorWindowPoints.ContainsKey(selectedRoomForDoor.ID))
                tempDoorWindowPoints[selectedRoomForDoor.ID] = new List<(WallLine, GameObject, GameObject)>();

            tempDoorWindowPoints[selectedRoomForDoor.ID].Add((door, p1Obj, p2Obj));

            Debug.Log($"[InsertDoorOrWindow] Đã thêm {type}: {p1} -> {p2}");

            string roomID = selectedRoomForDoor.ID;

            // Reset
            firstDoorPoint = null;
            selectedWallLineForDoor = null;
            selectedRoomForDoor = null;

            RedrawAllRooms();

            // Cập nhật lại mesh sàn (dù checkpoint không đổi, vẫn gọi vì có thể cần render lại mặt sàn)
            var floorGO = GameObject.Find($"RoomFloor_{roomID}");
            if (floorGO != null)
            {
                var meshCtrl = floorGO.GetComponent<RoomMeshController>();
                if (meshCtrl != null)
                    meshCtrl.GenerateMesh(RoomStorage.GetRoomByID(roomID).checkpoints);
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy RoomFloor_{roomID} để cập nhật mesh.");
            }
        }
    }

    Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    public void RedrawAllRooms()
    {
        DrawingTool.ClearAllLines();

        foreach (Room room in RoomStorage.rooms)
        {
            foreach (var wl in room.wallLines)
            {
                DrawingTool.currentLineType = wl.type;
                DrawingTool.DrawLineAndDistance(wl.start, wl.end);
            }
        }
    }

    bool TrySelectCheckpoint(Vector3 position)
    {
        float minDistance = closeThreshold;
        GameObject nearestCheckpoint = null;

        // Duyệt tất cả checkpoint trong allCheckpoints
        foreach (var loop in allCheckpoints)
        {
            foreach (var checkpoint in loop)
            {
                float distance = Vector3.Distance(checkpoint.transform.position, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCheckpoint = checkpoint;
                }
            }
        }

        // Nếu chưa có loop, dùng currentCheckpoints (đang vẽ)
        if (!isClosedLoop)
        {
            foreach (var checkpoint in currentCheckpoints)
            {
                float distance = Vector3.Distance(checkpoint.transform.position, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCheckpoint = checkpoint;
                }
            }
        }

        // Nếu không chọn được trong loop, thử với point cửa/cửa sổ
        foreach (var kvp in tempDoorWindowPoints)
        {
            foreach (var (line, p1GO, p2GO) in kvp.Value)
            {
                float dist1 = Vector3.Distance(p1GO.transform.position, position);
                if (dist1 < minDistance)
                {
                    minDistance = dist1;
                    nearestCheckpoint = p1GO;
                }

                float dist2 = Vector3.Distance(p2GO.transform.position, position);
                if (dist2 < minDistance)
                {
                    minDistance = dist2;
                    nearestCheckpoint = p2GO;
                }
            }
        }

        if (nearestCheckpoint != null)
        {
            selectedCheckpoint = nearestCheckpoint; // Luôn luôn gán vào selectedCheckpoint

            bool isExtra = nearestCheckpoint.CompareTag("CheckpointExtra");

            if (isExtra)
            {
                if (selectedExtraCheckpoint == null && selectedNormalCheckpoint == null)
                {
                    selectedExtraCheckpoint = nearestCheckpoint;
                    Debug.Log($"[Extra→Chọn điểm phụ đầu]: {nearestCheckpoint.transform.position}");
                }
                else if (selectedExtraCheckpoint != null && selectedExtraCheckpoint != nearestCheckpoint)
                {
                    ToggleConnectionBetweenCheckpoints(selectedExtraCheckpoint, nearestCheckpoint);
                    selectedExtraCheckpoint = null;
                }
                else if (selectedNormalCheckpoint != null)
                {
                    ToggleConnectionBetweenCheckpoints(selectedNormalCheckpoint, nearestCheckpoint);
                    selectedNormalCheckpoint = null;
                }
                else
                {
                    selectedExtraCheckpoint = null;
                }
            }
            else // không phải extra
            {
                if (selectedExtraCheckpoint != null)
                {
                    ToggleConnectionBetweenCheckpoints(selectedExtraCheckpoint, nearestCheckpoint);
                    selectedExtraCheckpoint = null;
                }
                else if (selectedNormalCheckpoint == null)
                {
                    selectedNormalCheckpoint = nearestCheckpoint;
                    Debug.Log($"[Normal→Chọn điểm chính đầu]: {nearestCheckpoint.transform.position}");
                }
                else if (selectedNormalCheckpoint == nearestCheckpoint)
                {
                    selectedNormalCheckpoint = null;
                }
            }

            return true;
        }

        return false;
    }

    void ToggleConnectionBetweenCheckpoints(GameObject pointA, GameObject pointB)
    {
        Vector3 start = pointA.transform.position;
        Vector3 end = pointB.transform.position;

        // === Tìm room chứa cả 2 điểm ===
        string roomID = FindRoomIDByPoint(start);
        if (string.IsNullOrEmpty(roomID)) return;

        if (!RoomFloorMap.TryGetValue(roomID, out GameObject floorGO)) return;
        Room room = RoomStorage.GetRoomByID(roomID);
        if (room == null) return;

        Vector2 localA = new Vector2(start.x, start.z) -
                         new Vector2(floorGO.transform.position.x, floorGO.transform.position.z);
        Vector2 localB = new Vector2(end.x, end.z) -
                         new Vector2(floorGO.transform.position.x, floorGO.transform.position.z);

        // === Thêm điểm phụ nếu chưa có ===
        if (!room.extraCheckpoints.Any(p => Vector2.Distance(p, localA) < 0.01f))
            room.extraCheckpoints.Add(localA);

        if (!room.extraCheckpoints.Any(p => Vector2.Distance(p, localB) < 0.01f))
            room.extraCheckpoints.Add(localB);

        // === Kiểm tra xem đã tồn tại line chưa ===
        WallLine existingLine = room.wallLines.FirstOrDefault(w =>
            (Vector3.Distance(w.start, start) < 0.01f && Vector3.Distance(w.end, end) < 0.01f) ||
            (Vector3.Distance(w.start, end) < 0.01f && Vector3.Distance(w.end, start) < 0.01f)
        );

        if (existingLine != null)
        {
            // Nếu đã tồn tại → xóa line
            room.wallLines.Remove(existingLine);
            Debug.Log($"[Disconnect] Gỡ nối {pointA.name} ↔ {pointB.name}");
        }
        else
        {
            // Nếu chưa tồn tại → thêm line
            WallLine manualLine = new WallLine(start, end, LineType.Wall);
            manualLine.isManualConnection = true; // <--- bạn cần thêm biến này vào class WallLine
            room.wallLines.Add(manualLine);
        }

        RoomStorage.UpdateOrAddRoom(room);
        DrawingTool.ClearAllLines();
        RedrawAllRooms();
    }

    public void MoveSelectedCheckpoint()
    {
        if (IsClickingOnBackgroundBlackUI(Input.mousePosition))
        {
            Debug.Log("Đang nhấn Background Black ➜ Không move checkpoint");
            return;
        }

        if (selectedCheckpoint == null) return;

        Vector3 newPosition = GetWorldPositionFromScreen(Input.mousePosition);
        Vector3 oldWorldPos = selectedCheckpoint.transform.position;

        // MoveSelectedCheckpointExtra();      
        if (selectedCheckpoint.CompareTag("CheckpointExtra"))
        {
            if (MoveSelectedCheckpointExtra()) return;
        }  

        // === Điểm cửa/cửa sổ
        foreach (var kvp in tempDoorWindowPoints)
        {
            foreach (var (line, p1GO, p2GO) in kvp.Value)
            {
                if (selectedCheckpoint == p1GO || selectedCheckpoint == p2GO)
                {
                    WallLine wall = FindClosestWallLine(line, kvp.Key);
                    if (wall == null) return;

                    Vector3 projected = ProjectPointOnLineSegment(wall.start, wall.end, newPosition);
                    if (selectedCheckpoint == p1GO) line.start = projected;
                    else line.end = projected;

                    selectedCheckpoint.transform.position = projected;
                    RedrawAllRooms();
                    return;
                }
            }
        }

        // === Di chuyển điểm chính trong polygon ===
        selectedCheckpoint.transform.position = newPosition;

        foreach (var loop in allCheckpoints)
        {
            if (!loop.Contains(selectedCheckpoint)) continue;

            string roomID = FindRoomIDForLoop(loop);
            if (string.IsNullOrEmpty(roomID)) return;
            Room room = RoomStorage.GetRoomByID(roomID);
            if (room == null) return;

            // Cập nhật line thủ công liên quan khi point chính di chuyển
            foreach (var line in room.wallLines)
            {
                if (!line.isManualConnection) continue;

                // Nếu start nối tới point chính
                if (Vector3.Distance(line.start, oldWorldPos) < 0.01f)
                {
                    line.start = newPosition;

                    // Kiểm tra end có phải đang nối tới một point phụ?
                    foreach (var extra in currentCheckpoints.Where(p => p.CompareTag("CheckpointExtra")))
                    {
                        if (Vector3.Distance(line.end, extra.transform.position) < 0.01f)
                        {
                            line.end = extra.transform.position; // fix lại cho khớp đúng
                            break;
                        }
                    }
                }
                // Nếu end nối tới point chính
                else if (Vector3.Distance(line.end, oldWorldPos) < 0.01f)
                {
                    line.end = newPosition;

                    // Kiểm tra start có phải đang nối tới một point phụ?
                    foreach (var extra in currentCheckpoints.Where(p => p.CompareTag("CheckpointExtra")))
                    {
                        if (Vector3.Distance(line.start, extra.transform.position) < 0.01f)
                        {
                            line.start = extra.transform.position; // fix lại
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < loop.Count; i++)
            {
                Vector3 pos = loop[i].transform.position;
                room.checkpoints[i] = new Vector2(pos.x, pos.z);
            }

            // Cập nhật wallLine chính (bỏ qua line thủ công)
            int wallCount = room.checkpoints.Count;
            int wallLineIndex = 0;
            for (int i = 0; i < room.wallLines.Count; i++)
            {
                if (room.wallLines[i].type != LineType.Wall) continue;
                if (room.wallLines[i].isManualConnection) continue;

                Vector2 p1 = room.checkpoints[wallLineIndex % wallCount];
                Vector2 p2 = room.checkpoints[(wallLineIndex + 1) % wallCount];

                room.wallLines[i].start = new Vector3(p1.x, 0, p1.y);
                room.wallLines[i].end = new Vector3(p2.x, 0, p2.y);

                wallLineIndex++;
            }

            // Cập nhật line thủ công liên quan
            Vector3 newWorldPos = selectedCheckpoint.transform.position;

            foreach (var line in room.wallLines)
            {
                if (!line.isManualConnection) continue;

                // Đảm bảo chỉ update đúng point liên quan
                if (Vector3.Distance(line.start, oldWorldPos) < 0.01f &&
                    Vector3.Distance(line.end, newWorldPos) > 0.01f)
                {
                    line.start = newWorldPos;
                }
                else if (Vector3.Distance(line.end, oldWorldPos) < 0.01f &&
                        Vector3.Distance(line.start, newWorldPos) > 0.01f)
                {
                    line.end = newWorldPos;
                }
            }

            // Cập nhật cửa sổ/cửa
            foreach (var door in room.wallLines)
            {
                if (door.type == LineType.Wall) continue;

                WallLine parentWall = null;
                float minDistance = float.MaxValue;
                foreach (var wall in room.wallLines)
                {
                    if (wall.type != LineType.Wall) continue;

                    float dist = GetDistanceFromSegment(door.start, wall.start, wall.end)
                                + GetDistanceFromSegment(door.end, wall.start, wall.end);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        parentWall = wall;
                    }
                }

                if (parentWall == null) continue;

                float r1 = Mathf.Clamp01(GetRatioAlongLine(door.start, parentWall.start, parentWall.end));
                float r2 = Mathf.Clamp01(GetRatioAlongLine(door.end, parentWall.start, parentWall.end));

                door.start = Vector3.Lerp(parentWall.start, parentWall.end, r1);
                door.end = Vector3.Lerp(parentWall.start, parentWall.end, r2);

                if (tempDoorWindowPoints.TryGetValue(room.ID, out var doorsInRoom))
                {
                    foreach (var (line, p1GO, p2GO) in doorsInRoom)
                    {
                        p1GO.transform.position = line.start;
                        p2GO.transform.position = line.end;
                    }
                }
            }

            RoomStorage.UpdateOrAddRoom(room);

            var floorGO = GameObject.Find($"RoomFloor_{roomID}");
            if (floorGO != null)
            {
                floorGO.GetComponent<RoomMeshController>()?.GenerateMesh(room.checkpoints);
                RefreshManualLinePositions(room);
            }

            DrawingTool.ClearAllLines();
            RedrawAllRooms();
            break;
        }
    }
    public bool MoveSelectedCheckpointExtra()
    {
        Vector3 newPosition = GetWorldPositionFromScreen(Input.mousePosition);
        Vector3 oldWorldPos = selectedCheckpoint.transform.position;

        isMovingCheckpoint = true;

        if (!selectedCheckpoint.CompareTag("CheckpointExtra"))
            return false;

        string roomID = FindRoomIDByPoint(oldWorldPos);
        if (string.IsNullOrEmpty(roomID))
            return false;

        Room room = RoomStorage.GetRoomByID(roomID);
        if (room == null || !RoomFloorMap.TryGetValue(room.ID, out GameObject floorGO))
            return false;

        Vector2 new2D = new Vector2(newPosition.x, newPosition.z);
        Vector2 local2D = new2D - new Vector2(floorGO.transform.position.x, floorGO.transform.position.z);
        Vector2 oldLocal2D = new Vector2(oldWorldPos.x, oldWorldPos.z) - new Vector2(floorGO.transform.position.x, floorGO.transform.position.z);

        if (!IsPointInPolygon(new2D, room.checkpoints))
        {
            Debug.LogWarning("Không cho phép kéo CheckpointExtra ra ngoài room.");
            return false;
        }

        float minDist = float.MaxValue;
        int insertIndex = -1;
        for (int i = 0; i < room.checkpoints.Count; i++)
        {
            Vector2 a = room.checkpoints[i];
            Vector2 b = room.checkpoints[(i + 1) % room.checkpoints.Count];
            float dist = GetDistanceFromSegment(new2D, a, b);
            if (dist < 0.2f && dist < minDist)
            {
                minDist = dist;
                insertIndex = i + 1;
            }
        }

        if (insertIndex != -1)
        {
            // === CONVERTING EXTRA TO MAIN CHECKPOINT ===

            // Step 1: Find and remove from extraCheckpoints
            int nearestExtraIndex = -1;
            float minDistExtra = float.MaxValue;
            for (int i = 0; i < room.extraCheckpoints.Count; i++)
            {
                float dist = Vector2.Distance(room.extraCheckpoints[i], oldLocal2D);
                if (dist < minDistExtra)
                {
                    minDistExtra = dist;
                    nearestExtraIndex = i;
                }
            }

            if (nearestExtraIndex != -1)
                room.extraCheckpoints.RemoveAt(nearestExtraIndex);

            // Step 2: Calculate final world position FIRST
            Vector3 worldPosAfterMove = RoomToWorld(local2D, floorGO);

            // Step 3: Store all manual lines that connect to this checkpoint WITH EXACT COORDINATES
            List<WallLineConnection> connectionsToUpdate = new List<WallLineConnection>();
            foreach (var line in room.wallLines)
            {
                if (!line.isManualConnection) continue;

                bool startConnected = Vector3.Distance(line.start, oldWorldPos) < 0.01f;
                bool endConnected = Vector3.Distance(line.end, oldWorldPos) < 0.01f;

                if (startConnected || endConnected)
                {
                    connectionsToUpdate.Add(new WallLineConnection
                    {
                        line = line,
                        isStartPoint = startConnected,
                        isEndPoint = endConnected,
                        newPosition = worldPosAfterMove
                    });
                }
            }

            // Step 4: Insert checkpoint and update transform
            room.checkpoints.Insert(insertIndex, local2D);
            selectedCheckpoint.transform.position = worldPosAfterMove;
            selectedCheckpoint.tag = "Untagged";
            selectedCheckpoint.transform.SetParent(null);

            // Step 5: Update allCheckpoints list
            var loop = allCheckpoints.Find(l => FindRoomIDForLoop(l) == room.ID);
            if (loop != null)
                loop.Insert(insertIndex, selectedCheckpoint);

            // Step 6: Apply stored connections BEFORE rebuilding walls
            foreach (var connection in connectionsToUpdate)
            {
                if (connection.isStartPoint)
                    connection.line.start = connection.newPosition;
                if (connection.isEndPoint)
                    connection.line.end = connection.newPosition;
            }

            // Step 7: Create backup of ALL manual lines (including updated ones)
            List<WallLine> manualLinesBackup = room.wallLines
                .Where(w => w.isManualConnection)
                .Select(w => new WallLine
                {
                    start = w.start,
                    end = w.end,
                    type = w.type,
                    isManualConnection = true
                }).ToList();

            // Step 8: Rebuild perimeter walls only (preserve manual connections)
            RebuildWallLinesPreservingDoors(room);

            // Step 9: Re-add manual lines with updated coordinates
            room.wallLines.AddRange(manualLinesBackup);

            // Step 10: Final precision snap - ensure ALL endpoints are exactly at checkpoint positions
            if (placedPointsByRoom.TryGetValue(room.ID, out List<GameObject> checkpointGOs))
            {
                foreach (var line in room.wallLines)
                {
                    if (!line.isManualConnection) continue;

                    // Snap to exact checkpoint positions
                    foreach (var cp in checkpointGOs)
                    {
                        Vector3 cpPos = cp.transform.position;

                        // More generous tolerance for the moved checkpoint
                        float tolerance = (cp == selectedCheckpoint) ? 0.15f : 0.05f;

                        if (Vector3.Distance(line.start, cpPos) < tolerance)
                            line.start = cpPos;
                        if (Vector3.Distance(line.end, cpPos) < tolerance)
                            line.end = cpPos;
                    }
                }
            }

            // Step 11: Update storage and regenerate visuals
            RoomStorage.UpdateOrAddRoom(room);
            floorGO.GetComponent<RoomMeshController>()?.GenerateMesh(room.checkpoints);
            DrawingTool.ClearAllLines();
            RedrawAllRooms();
            return true;
        }
        else
        {
            // === MOVING WITHIN EXTRA CHECKPOINTS ===
            int nearestIndex = -1;
            float minDist1 = float.MaxValue;
            for (int i = 0; i < room.extraCheckpoints.Count; i++)
            {
                float dist = Vector2.Distance(room.extraCheckpoints[i], oldLocal2D);
                if (dist < minDist1)
                {
                    minDist1 = dist;
                    nearestIndex = i;
                }
            }

            if (nearestIndex != -1 && minDist1 < 0.5f)
            {
                Vector3 worldPosAfterMove = RoomToWorld(local2D, floorGO);

                // Update extra checkpoint position
                room.extraCheckpoints[nearestIndex] = local2D;
                selectedCheckpoint.transform.position = worldPosAfterMove;

                // Update manual wall lines that connect to this extra checkpoint
                foreach (var line in room.wallLines)
                {
                    if (!line.isManualConnection) continue;
                    if (Vector3.Distance(line.start, oldWorldPos) < 0.01f)
                        line.start = worldPosAfterMove;
                    if (Vector3.Distance(line.end, oldWorldPos) < 0.01f)
                        line.end = worldPosAfterMove;
                }

                UpdateWallLinesFromExtraCheckpoint(room, oldLocal2D, local2D, floorGO);
                UpdateExtraCheckpointVisual(room.ID, nearestIndex, local2D);
            }

            RoomStorage.UpdateOrAddRoom(room);
            DrawingTool.ClearAllLines();
            RedrawAllRooms();
            return true;
        }
    }

    // Helper class to track line connections
    public class WallLineConnection
    {
        public WallLine line;
        public bool isStartPoint;
        public bool isEndPoint;
        public Vector3 newPosition;
    }

    // Enhanced RebuildWallLinesPreservingDoors - only rebuild perimeter, preserve manual
    void RebuildWallLinesPreservingDoors(Room room)
    {
        List<WallLine> oldWalls = new List<WallLine>(room.wallLines);

        // Preserve door/window information
        var preservedDoorWindowLines = oldWalls
            .Where(w => w.type != LineType.Wall)
            .Select(dw =>
            {
                WallLine parent = oldWalls.FirstOrDefault(w =>
                    w.type == LineType.Wall && !w.isManualConnection &&
                    GetDistanceFromSegment(dw.start, w.start, w.end) +
                    GetDistanceFromSegment(dw.end, w.start, w.end) < 0.1f);

                if (parent == null) return (null, 0f, 0f, dw);

                float r1 = GetRatioAlongLine(dw.start, parent.start, parent.end);
                float r2 = GetRatioAlongLine(dw.end, parent.start, parent.end);
                return (parent, r1, r2, dw);
            })
            .Where(p => p.parent != null)
            .ToList();

        // Remove ONLY non-manual walls and doors/windows
        room.wallLines.RemoveAll(w => !w.isManualConnection);

        // Rebuild perimeter walls
        for (int i = 0; i < room.checkpoints.Count; i++)
        {
            Vector2 p1 = room.checkpoints[i];
            Vector2 p2 = room.checkpoints[(i + 1) % room.checkpoints.Count];

            Vector3 start = new Vector3(p1.x, 0, p1.y);
            Vector3 end = new Vector3(p2.x, 0, p2.y);

            // Check if this wall already exists (from previous rebuild)
            bool wallExists = room.wallLines.Any(existing =>
                existing.type == LineType.Wall &&
                ((Vector3.Distance(existing.start, start) < 0.01f && Vector3.Distance(existing.end, end) < 0.01f) ||
                 (Vector3.Distance(existing.start, end) < 0.01f && Vector3.Distance(existing.end, start) < 0.01f)));

            if (!wallExists)
            {
                room.wallLines.Add(new WallLine(start, end, LineType.Wall));
            }
        }

        // Restore doors and windows on perimeter walls
        foreach (var (oldParent, r1, r2, dw) in preservedDoorWindowLines)
        {
            WallLine newWall = room.wallLines.FirstOrDefault(w =>
                w.type == LineType.Wall && !w.isManualConnection &&
                Vector3.Distance(w.start, oldParent.start) < 0.1f &&
                Vector3.Distance(w.end, oldParent.end) < 0.1f);

            if (newWall != null)
            {
                Vector3 newStart = Vector3.Lerp(newWall.start, newWall.end, Mathf.Clamp01(r1));
                Vector3 newEnd = Vector3.Lerp(newWall.start, newWall.end, Mathf.Clamp01(r2));
                room.wallLines.Add(new WallLine(newStart, newEnd, dw.type, dw.distanceHeight, dw.Height));
            }
        }

        // Update tempDoorWindowPoints if needed
        if (tempDoorWindowPoints.TryGetValue(room.ID, out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                var (_, p1, p2) = list[i];
                var newLine = room.wallLines.FirstOrDefault(w =>
                    (w.type == LineType.Door || w.type == LineType.Window) &&
                    Vector3.Distance(w.start, p1.transform.position) < 0.1f &&
                    Vector3.Distance(w.end, p2.transform.position) < 0.1f);

                if (newLine != null)
                    list[i] = (newLine, p1, p2);
            }
        }
    }

    void RefreshManualLinePositions(Room room)
    {
        if (!RoomFloorMap.TryGetValue(room.ID, out GameObject floorGO)) return;

        foreach (var line in room.wallLines)
        {
            if (!line.isManualConnection) continue;

            foreach (var local in room.extraCheckpoints)
            {
                Vector3 world = new Vector3(local.x, 0, local.y) + floorGO.transform.position;

                if (Vector3.Distance(line.start, world) < 0.05f)
                    line.start = world;
                if (Vector3.Distance(line.end, world) < 0.05f)
                    line.end = world;
            }
        }
    }

    Vector3 RoomToWorld(Vector2 localPos, GameObject floorGO)
    {
        return new Vector3(localPos.x, 0, localPos.y) + floorGO.transform.position;
    }

    void UpdateWallLinesFromExtraCheckpoint(Room room, Vector2 oldLocal, Vector2 newLocal, GameObject floorGO)
    {
        Vector3 oldWorld = RoomToWorld(oldLocal, floorGO);
        Vector3 newWorld = RoomToWorld(newLocal, floorGO);

        foreach (var line in room.wallLines)
        {
            if (!line.isManualConnection) continue;

            if (Vector3.Distance(line.start, oldWorld) < 0.01f)
                line.start = newWorld;

            if (Vector3.Distance(line.end, oldWorld) < 0.01f)
                line.end = newWorld;
        }
    }

    // Dictionary<string, List<GameObject>> ExtraCheckpointVisuals;
    Dictionary<string, List<GameObject>> ExtraCheckpointVisuals = new Dictionary<string, List<GameObject>>();

    void UpdateExtraCheckpointVisual(string roomID, int index, Vector2 local2D)
    {
        if (!ExtraCheckpointVisuals.TryGetValue(roomID, out var visuals))
        {
            return;
        }

        if (index < 0 || index >= visuals.Count)
        {
            return;
        }

        if (!RoomFloorMap.TryGetValue(roomID, out var floor))
        {
            return;
        }

        if (visuals[index] == null)
        {
            return;
        }

        visuals[index].transform.position = new Vector3(local2D.x, 0f, local2D.y) + floor.transform.position;
    }

    // void RebuildWallLinesPreservingDoors(Room room)
    // {
    //     // 1. Backup all old wall lines
    //     List<WallLine> oldWalls = new List<WallLine>(room.wallLines);

    //     // 2. Backup cửa/cửa sổ kèm parent wall + tỷ lệ theo wall gốc
    //     var preservedDoorWindowLines = oldWalls
    //         .Where(w => w.type != LineType.Wall)
    //         .Select(dw =>
    //         {
    //             WallLine parent = oldWalls
    //                 .FirstOrDefault(w => w.type == LineType.Wall &&
    //                                      GetDistanceFromSegment(dw.start, w.start, w.end) +
    //                                      GetDistanceFromSegment(dw.end, w.start, w.end) < 0.1f);

    //             if (parent == null) return (null, 0f, 0f, dw);

    //             float r1 = GetRatioAlongLine(dw.start, parent.start, parent.end);
    //             float r2 = GetRatioAlongLine(dw.end, parent.start, parent.end);

    //             return (parent, r1, r2, dw);
    //         })
    //         .Where(p => p.parent != null)
    //         .ToList();

    //     // 3. Rebuild wall lines (Wall only)
    //     room.wallLines.Clear();
    //     for (int i = 0; i < room.checkpoints.Count; i++)
    //     {
    //         Vector2 p1 = room.checkpoints[i];
    //         Vector2 p2 = room.checkpoints[(i + 1) % room.checkpoints.Count];

    //         Vector3 start = new Vector3(p1.x, 0, p1.y);
    //         Vector3 end = new Vector3(p2.x, 0, p2.y);

    //         var existing = oldWalls.FirstOrDefault(w =>
    //             (Vector3.Distance(w.start, start) < 0.01f && Vector3.Distance(w.end, end) < 0.01f) ||
    //             (Vector3.Distance(w.start, end) < 0.01f && Vector3.Distance(w.end, start) < 0.01f));

    //         if (existing != null && existing.type == LineType.Wall)
    //             room.wallLines.Add(new WallLine(existing));
    //         else
    //             room.wallLines.Add(new WallLine(start, end, LineType.Wall));
    //     }

    //     // 3.5. Thêm lại các wallLine nối từ extraCheckpoints
    //     foreach (var w in oldWalls)
    //     {
    //         if (w.type != LineType.Wall || !w.isManualConnection) continue;

    //         // Thêm vào nếu chưa tồn tại
    //         bool exists = room.wallLines.Any(existing =>
    //             (Vector3.Distance(existing.start, w.start) < 0.01f && Vector3.Distance(existing.end, w.end) < 0.01f) ||
    //             (Vector3.Distance(existing.start, w.end) < 0.01f && Vector3.Distance(existing.end, w.start) < 0.01f));

    //         if (!exists)
    //         {
    //             room.wallLines.Add(new WallLine(w)); // copy lại line thủ công
    //         }
    //     }

    //     // 4. Chèn lại các cửa/cửa sổ dựa trên tỷ lệ theo đoạn wall mới
    //     foreach (var (oldParent, r1, r2, dw) in preservedDoorWindowLines)
    //     {
    //         WallLine newWall = room.wallLines.FirstOrDefault(w =>
    //             w.type == LineType.Wall &&
    //             Vector3.Distance(w.start, oldParent.start) < 0.1f &&
    //             Vector3.Distance(w.end, oldParent.end) < 0.1f);

    //         if (newWall == null) continue;

    //         Vector3 newStart = Vector3.Lerp(newWall.start, newWall.end, Mathf.Clamp01(r1));
    //         Vector3 newEnd = Vector3.Lerp(newWall.start, newWall.end, Mathf.Clamp01(r2));

    //         room.wallLines.Add(new WallLine(newStart, newEnd, dw.type, dw.distanceHeight, dw.Height));
    //     }

    //     // 5. Cập nhật lại tempDoorWindowPoints để giữ reference chính xác
    //     if (tempDoorWindowPoints.TryGetValue(room.ID, out var list))
    //     {
    //         for (int i = 0; i < list.Count; i++)
    //         {
    //             var (_, p1, p2) = list[i];

    //             // Tìm lại door mới từ wallLines
    //             var newLine = room.wallLines.FirstOrDefault(w =>
    //                 (w.type == LineType.Door || w.type == LineType.Window) &&
    //                 Vector3.Distance(w.start, p1.transform.position) < 0.1f &&
    //                 Vector3.Distance(w.end, p2.transform.position) < 0.1f);

    //             if (newLine != null)
    //             {
    //                 list[i] = (newLine, p1, p2); // gán lại line mới vào tuple
    //             }
    //         }
    //     }

    //     // === Cập nhật lại mesh sàn sau khi checkpoint thay đổi
    //     if (RoomFloorMap.TryGetValue(room.ID, out GameObject floorGO))
    //     {
    //         var allExtraWorldPoints = room.extraCheckpoints
    //             .Select(local => new Vector3(local.x, 0, local.y) + floorGO.transform.position)
    //             .ToList();

    //         var allMainWorldPoints = room.checkpoints
    //             .Select(p => new Vector3(p.x, 0, p.y) + floorGO.transform.position)
    //             .ToList();

    //         foreach (var line in room.wallLines)
    //         {
    //             if (!line.isManualConnection) continue;

    //             foreach (var wp in allExtraWorldPoints)
    //             {
    //                 if (Vector3.Distance(line.start, wp) < 0.05f)
    //                     line.start = wp;
    //                 if (Vector3.Distance(line.end, wp) < 0.05f)
    //                     line.end = wp;
    //             }

    //             foreach (var wp in allMainWorldPoints)
    //             {
    //                 if (Vector3.Distance(line.start, wp) < 0.05f)
    //                     line.start = wp;
    //                 if (Vector3.Distance(line.end, wp) < 0.05f)
    //                     line.end = wp;
    //             }
    //         }

    //         // Cập nhật lại mesh
    //         floorGO.GetComponent<RoomMeshController>()?.GenerateMesh(room.checkpoints);
    //     }
    // }

    private WallLine FindClosestWallLine(WallLine doorLine, string roomID)
    {
        var room = RoomStorage.GetRoomByID(roomID);
        if (room == null) return null;

        WallLine closest = null;
        float minDist = float.MaxValue;

        foreach (var wall in room.wallLines)
        {
            if (wall.type != LineType.Wall) continue;

            float dist = GetDistanceFromSegment(doorLine.start, wall.start, wall.end)
                         + GetDistanceFromSegment(doorLine.end, wall.start, wall.end);

            if (dist < minDist)
            {
                minDist = dist;
                closest = wall;
            }
        }

        return closest;
    }

    private float GetDistanceFromSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 projected = ProjectPointOnLineSegment(a, b, point);
        return Vector3.Distance(point, projected);
    }

    private float GetRatioAlongLine(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = point - a;
        return Vector3.Dot(ap, ab) / ab.sqrMagnitude;
    }

    public string FindRoomIDForLoop(List<GameObject> loop)
    {
        foreach (var mapping in loopMappings)
        {
            if (ReferenceEquals(mapping.CheckpointsGO, loop)) return mapping.RoomID;
        }

        Debug.LogWarning("Loop không tìm thấy RoomID!");
        return null;
    }

    public void DeselectCheckpoint()
    {
        selectedCheckpoint = null;
        isMovingCheckpoint = false;
    }

    public Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Mặt phẳng ngang y=0
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return ray.GetPoint(5f);
    }

    // === Load points from RoomStorage
    void LoadPointsFromRoomStorage()
    {
        var rooms = RoomStorage.rooms;
        if (rooms.Count == 0)
        {
            Debug.Log("Không có Room nào để hiển thị.");
            return;
        }

        foreach (var room in rooms)
        {
            // === Tạo lại checkpoint GameObject từ room.checkpoints
            List<GameObject> loopGO = new List<GameObject>();
            foreach (var pt in room.checkpoints)
            {
                Vector3 worldPos = new Vector3(pt.x, 0, pt.y);
                GameObject cp = Instantiate(checkpointPrefab, worldPos, Quaternion.identity);
                loopGO.Add(cp);
            }

            // === Lưu vào ánh xạ checkpoint<->RoomID
            allCheckpoints.Add(loopGO);
            loopMappings.Add(new LoopMap(room.ID, loopGO));

            // === Tạo lại mesh sàn (có thể drag)
            GameObject floorGO = new GameObject($"RoomFloor_{room.ID}");
            floorGO.transform.position = Vector3.zero;
            floorGO.transform.rotation = Quaternion.identity;
            floorGO.transform.localScale = Vector3.one;
            var meshCtrl = floorGO.AddComponent<RoomMeshController>();
            meshCtrl.Initialize(room.ID); // tự gọi GenerateMesh(room.checkpoints)

            // === Vẽ lại các wallLines
            foreach (var wl in room.wallLines)
            {
                DrawingTool.currentLineType = wl.type;
                DrawingTool.DrawLineAndDistance(wl.start, wl.end);

                // Nếu là cửa hoặc cửa sổ: tạo 2 điểm đầu/cuối riêng
                if (wl.type == LineType.Door || wl.type == LineType.Window)
                {
                    GameObject p1 = Instantiate(checkpointPrefab, wl.start, Quaternion.identity);
                    GameObject p2 = Instantiate(checkpointPrefab, wl.end, Quaternion.identity);
                    p1.name = $"{wl.type}_P1";
                    p2.name = $"{wl.type}_P2";

                    if (!tempDoorWindowPoints.ContainsKey(room.ID))
                        tempDoorWindowPoints[room.ID] = new List<(WallLine, GameObject, GameObject)>();

                    tempDoorWindowPoints[room.ID].Add((wl, p1, p2));
                }
            }
        }

        Debug.Log($"[LoadPointsFromRoomStorage] Đã load lại {rooms.Count} phòng, {allCheckpoints.Count} loop.");
    }

    void ShowIncompleteLoopPopup()
    {
        // PopupController.Show(
        //     "Mạch chưa khép kín!\nBạn muốn xóa dữ liệu vẽ tạm không?",
        //     onYes: () =>
        //     {
        //         Debug.Log("Người dùng chọn YES: Xóa toàn bộ checkpoint + line.");
        //         DeleteCurrentDrawingData();
        //     },
        //     onNo: () =>
        //     {
        //         Debug.Log("Người dùng chọn NO: Tiếp tục vẽ để khép kín.");
        //     }
        // );
        //
        var popup = Instantiate(ModularPopup.Prefab);
        popup.AutoFindCanvasAndSetup();
        popup.Header = "Mạch chưa khép kín!\\nBạn muốn xóa dữ liệu vẽ tạm không?";
        popup.ClickYesEvent = () =>
        {
            Debug.Log("Người dùng chọn YES: Xóa toàn bộ checkpoint + line.");
            DeleteCurrentDrawingData();
        };
        popup.ClickNoEvent = () => { Debug.Log("Người dùng chọn NO: Tiếp tục vẽ để khép kín."); };
        popup.EventWhenClickButtons = () => { BackgroundUI.Instance.Hide(); };
        BackgroundUI.Instance.Show(popup.gameObject, null);

        popup.autoClearWhenClick = true;
    }

    public void DeleteCurrentDrawingData()
    {
        foreach (var cp in currentCheckpoints)
        {
            if (cp != null)
                Destroy(cp);
        }

        currentCheckpoints.Clear();

        wallLines.Clear();
        DrawingTool.ClearAllLines();

        isClosedLoop = false;
        previewCheckpoint = null;
        selectedCheckpoint = null;

        foreach (var list in tempDoorWindowPoints.Values)
        {
            foreach (var (line, p1, p2) in list)
            {
                if (p1 != null) Destroy(p1);
                if (p2 != null) Destroy(p2);
            }
        }

        tempDoorWindowPoints.Clear();

        Debug.Log("Đã xóa toàn bộ dữ liệu vẽ chưa khép kín.");
    }

    public string lastSelectedRoomID = null;

    public string GetSelectedRoomID()
    {
        if (selectedCheckpoint != null)
        {
            foreach (var loop in allCheckpoints)
            {
                if (loop.Contains(selectedCheckpoint))
                {
                    lastSelectedRoomID = FindRoomIDForLoop(loop);
                    return lastSelectedRoomID;
                }
            }
        }

        // Nếu đang kéo mesh ➜ lấy RoomID từ RoomMeshController đang hoạt động
        if (IsDraggingRoom)
        {
            var activeFloors = GameObject.FindObjectsByType<RoomMeshController>(FindObjectsSortMode.None);
            foreach (var floor in activeFloors)
            {
                if (floor.isDragging) // đã gán từ RoomMeshController
                {
                    lastSelectedRoomID = floor.RoomID;
                    return lastSelectedRoomID;
                }
            }
        }

        // Nếu đang không chọn gì nhưng vẫn có room đã chọn trước đó → giữ nguyên
        return lastSelectedRoomID;
    }

    // === Hàm ko cho move trên UI
    private bool IsClickingOnBackgroundBlackUI(Vector2 screenPosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == "Background Black")
            {
                Debug.Log("Click UI trên Background Black ➜ Không cho move point");
                return true;
            }
        }

        return false;
    }

    public void CreateRegularPolygonRoom(int sides, float edgeLength)
    {
        if (sides < 3)
        {
            Debug.LogError("Số cạnh phải >= 3");
            return;
        }

        if (edgeLength <= 0)
        {
            Debug.LogError("Chiều dài cạnh phải > 0");
            return;
        }

        // Xoá dữ liệu tạm nếu đang vẽ dở
        // DeleteCurrentDrawingData();

        // Tìm center trên mặt phẳng y=0 theo camera
        Camera cam = drawingCamera != null ? drawingCamera : Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float enter = 0;
        Vector3 center = Vector3.zero;

        if (groundPlane.Raycast(ray, out enter))
        {
            center = ray.GetPoint(enter);
        }
        else
        {
            Debug.LogError("Không raycast được xuống mặt phẳng y=0");
            return;
        }

        Debug.Log($"Tâm room tại: {center}");

        // Tính bán kính từ chiều dài cạnh
        float radius = edgeLength / (2 * Mathf.Sin(Mathf.PI / sides));
        Debug.Log($"Bán kính: {radius}");

        // Tạo các checkpoint prefab
        float angleOffset = Mathf.PI / 2; // Quay để cạnh đầu hướng lên
        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * i / sides + angleOffset;
            float x = center.x + radius * Mathf.Cos(angle);
            float z = center.z + radius * Mathf.Sin(angle);
            Vector3 pos = new Vector3(x, 0, z);

            var cp = Instantiate(checkpointPrefab, pos, Quaternion.identity);
            currentCheckpoints.Add(cp);
        }

        // Tạo wallLines & vẽ line
        for (int i = 0; i < currentCheckpoints.Count; i++)
        {
            Vector3 p1 = currentCheckpoints[i].transform.position;
            Vector3 p2 = (i == currentCheckpoints.Count - 1)
                ? currentCheckpoints[0].transform.position
                : currentCheckpoints[i + 1].transform.position;

            DrawingTool.DrawLineAndDistance(p1, p2);
            wallLines.Add(new WallLine(p1, p2, LineType.Wall));
        }

        // Tạo Room & lưu
        Room newRoom = new Room();
        foreach (GameObject cp in currentCheckpoints)
        {
            Vector3 pos = cp.transform.position;
            pos.y = 0f;
            newRoom.checkpoints.Add(new Vector2(pos.x, pos.z));
        }

        if (MeshGenerator.CalculateArea(newRoom.checkpoints) > 0)
        {
            newRoom.checkpoints.Reverse();
            Debug.Log("Đã đảo chiều polygon để mesh đúng mặt.");
        }

        newRoom.wallLines.AddRange(wallLines);

        RoomStorage.rooms.Add(newRoom);

        // Tạo mesh sàn
        GameObject floorGO = new GameObject($"RoomFloor_{newRoom.ID}");
        RoomMeshController meshCtrl = floorGO.AddComponent<RoomMeshController>();
        meshCtrl.Initialize(newRoom.ID);

        // Ánh xạ loop
        List<GameObject> loopRef = new List<GameObject>(currentCheckpoints);
        allCheckpoints.Add(loopRef);
        loopMappings.Add(new LoopMap(newRoom.ID, loopRef));

        currentCheckpoints.Clear();
        wallLines.Clear();

        Debug.Log($"Đã tạo Room tự động: {sides} cạnh, cạnh dài ~{edgeLength}m, RoomID: {newRoom.ID}");
    }

    public void CreateRectangleRoom(float width, float height)
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Chiều dài và chiều rộng phải > 0");
            return;
        }

        // Xoá dữ liệu tạm nếu đang vẽ dở

        // Tìm center trên mặt phẳng y=0 theo camera
        Camera cam = drawingCamera != null ? drawingCamera : Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float enter = 0;
        Vector3 center = Vector3.zero;

        if (groundPlane.Raycast(ray, out enter))
        {
            center = ray.GetPoint(enter);
        }
        else
        {
            Debug.LogError("Không raycast được xuống mặt phẳng y=0");
            return;
        }

        Debug.Log($"Tâm room (rectangle) tại: {center}");

        // Tính 4 đỉnh hình chữ nhật quanh center
        CreateRectangleRoom(width, height, center,null,true);
    }

    public void CreateRectangleRoom(float width, float height, Vector3 center,string ID,bool isCreateCommand)
    {
        DeleteCurrentDrawingData();

        Vector3 p1 = new Vector3(center.x - width / 2, 0, center.z - height / 2);
        Vector3 p2 = new Vector3(center.x - width / 2, 0, center.z + height / 2);
        Vector3 p3 = new Vector3(center.x + width / 2, 0, center.z + height / 2);
        Vector3 p4 = new Vector3(center.x + width / 2, 0, center.z - height / 2);

        List<Vector3> corners = new List<Vector3> { p1, p2, p3, p4 };

        // Tạo checkpoint prefab tại từng góc
        foreach (Vector3 pos in corners)
        {
            var cp = Instantiate(checkpointPrefab, pos, Quaternion.identity);
            currentCheckpoints.Add(cp);
        }

        // Tạo wallLines & vẽ line
        for (int i = 0; i < currentCheckpoints.Count; i++)
        {
            Vector3 start = currentCheckpoints[i].transform.position;
            Vector3 end = (i == currentCheckpoints.Count - 1)
                ? currentCheckpoints[0].transform.position
                : currentCheckpoints[i + 1].transform.position;

            DrawingTool.DrawLineAndDistance(start, end);
            wallLines.Add(new WallLine(start, end, LineType.Wall));
        }

        // Tạo Room & lưu
        Room newRoom = new Room();
        if (!string.IsNullOrEmpty(ID))
        {
            newRoom.SetID(ID);
        }
        
        foreach (GameObject cp in currentCheckpoints)
        {
            Vector3 pos = cp.transform.position;
            newRoom.checkpoints.Add(new Vector2(pos.x, pos.z));
        }

        if (MeshGenerator.CalculateArea(newRoom.checkpoints) > 0)
        {
            newRoom.checkpoints.Reverse();
            Debug.Log("Đã đảo chiều polygon để mesh đúng mặt.");
        }

        newRoom.wallLines.AddRange(wallLines);
        RoomStorage.rooms.Add(newRoom);

        // Tạo mesh sàn
        GameObject floorGO = new GameObject($"RoomFloor_{newRoom.ID}");
        RoomMeshController meshCtrl = floorGO.AddComponent<RoomMeshController>();
        meshCtrl.Initialize(newRoom.ID);

        // Ánh xạ loop
        List<GameObject> loopRef = new List<GameObject>(currentCheckpoints);
        allCheckpoints.Add(loopRef);
        loopMappings.Add(new LoopMap(newRoom.ID, loopRef));

        currentCheckpoints.Clear();
        wallLines.Clear();

        DrawingTool.DrawAllLinesFromRoomStorage();
        Debug.Log($"Đã tạo Room hình chữ nhật: {width} x {height} m, RoomID: {newRoom.ID}");

        if (!isCreateCommand) return;
        
        var data = new RectangularCreatingData();
        data.width = width;
        data.heigh = height;
        data.RoomID = newRoom.ID;
        data.position = center;
        UndoRedoController.Instance.AddToUndo(new CreateRectangularCommand(data));
    }

}