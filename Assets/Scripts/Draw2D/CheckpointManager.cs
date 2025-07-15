using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CheckpointManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject checkpointPrefab;
    public DrawingTool DrawingTool;
    public PenManager penManager;
    public UndoRedoManager undoRedoManager;
    public StoragePermissionRequester permissionRequester;


    public LineType currentLineType = LineType.Wall;
    public List<WallLine> wallLines = new List<WallLine>();
    public List<Room> rooms = new List<Room>();


    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();
    private List<GameObject> currentCheckpoints = new List<GameObject>();
    private GameObject selectedCheckpoint = null; // Điểm được chọn để di chuyển    
    private float closeThreshold = 0.5f; // Khoảng cách tối đa để chọn điểm
    private bool isDragging = false; // Kiểm tra xem có đang kéo điểm không
    private Vector3 previewPosition; // Vị trí preview
    private bool isPreviewing = false; // Trạng thái preview
    private bool isClosedLoop = false; // Biến kiểm tra xem mạch đã khép kín chưa
    private GameObject previewCheckpoint = null;
    public List<List<GameObject>> AllCheckpoints => allCheckpoints; // Truy cập danh sách tất cả các checkpoint từ bên ngoài
    public bool flagDoor = false; // Bật để vẽ nét đứt (dành cho cửa)

    private WallLine selectedWallLineForDoor;   // đoạn tường được chọn
    private Room selectedRoomForDoor;
    private Vector3? firstDoorPoint = null;     // lưu P1
    // Map loop checkpoint list => Room ID
    private Dictionary<List<GameObject>, string> loopToRoomID = new Dictionary<List<GameObject>, string>();
    private List<LoopMap> loopMappings = new List<LoopMap>();
    List<GameObject> doorPoints = new List<GameObject>();

    void Start()
    {
        // LoadPointsFromDataTransfer();
        LoadPointsFromRoomStorage();
    }

    void Update()
    {
        if (!PenManager.isPenActive)
        {
            penManager.HandleZoomAndPan(true);

            if (Input.GetMouseButtonDown(0))
            {
                SelectCheckpoint();
            }
            else if (Input.GetMouseButton(0))
            {
                if (selectedCheckpoint != null)
                {
                    isDragging = true;
                    MoveSelectedCheckpoint();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                DeselectCheckpoint();
                isDragging = false;
            }

            return;
        }

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
        if (Input.GetMouseButtonDown(0)) // Nhấn để chọn điểm hoặc chuẩn bị đặt điểm
        {
            SelectCheckpoint();
        }
        else if (Input.GetMouseButton(0)) // Khi giữ ngón tay, di chuyển điểm hoặc hiển thị preview
        {
            if (IsInSavedLoop(selectedCheckpoint) || isClosedLoop) // Nếu đã chọn điểm và mạch kín, di chuyển điểm
            {
                MoveSelectedCheckpoint();
                isDragging = true;
            }
            else // Nếu chưa chọn điểm, hiển thị preview để đặt điểm mới
            {
                isPreviewing = true;
                previewPosition = GetWorldPositionFromScreen(Input.mousePosition);

                if (currentCheckpoints.Count > 0)
                {
                    Vector3 lastPoint = currentCheckpoints[^1].transform.position;
                    DrawingTool.DrawPreviewLine(lastPoint, previewPosition);
                }

                if (previewCheckpoint == null)
                {
                    previewCheckpoint = Instantiate(checkpointPrefab, previewPosition, Quaternion.identity);
                    previewCheckpoint.name = "PreviewCheckpoint";
                }
                previewCheckpoint.transform.position = previewPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0)) // Nhả chuột để đặt điểm hoặc bỏ chọn
        {
            isPreviewing = false;
            DrawingTool.ClearPreviewLine();

            if (previewCheckpoint != null)
            {
                Destroy(previewCheckpoint);
            }

            Vector3 clickPosition = GetWorldPositionFromScreen(Input.mousePosition);
            if (!isDragging) // Nếu không phải kéo điểm, đặt checkpoint mới
            {
                HandleCheckpointPlacement(previewPosition);
            }
            //tìm line để thêm checkpoint vào line đã có 
            // if (isClosedLoop)
            // {
            //     InsertCheckpointIntoExistingLoop(clickPosition);
            // }
            // else
            // {
            //     HandleCheckpointPlacement(clickPosition); // Vẽ bình thường
            // }

            DeselectCheckpoint();
            isDragging = false;
        }
    }
    bool IsInSavedLoop(GameObject checkpoint)
    {
        foreach (var loop in allCheckpoints)
        {
            if (loop.Contains(checkpoint))
                return true;
        }
        return false;
    }

    void SelectCheckpoint()
    {
        Vector3 clickPosition = GetWorldPositionFromScreen(Input.mousePosition);
        TrySelectCheckpoint(clickPosition);
    }

    void HandleCheckpointPlacement(Vector3 position)
    {
        if (selectedCheckpoint != null) return; // Nếu đã chọn điểm, không cần đặt mới

        // === Nếu đang định thêm Door/Window nhưng chưa có loop kín ===
        if ((currentLineType == LineType.Door || currentLineType == LineType.Window) && currentCheckpoints.Count > 0)
        {
            ShowIncompleteLoopPopup(); // hỏi xóa hay tiếp tục vẽ để khép
            return;
        }

        if (currentLineType == LineType.Door || currentLineType == LineType.Window)
        {
            // Nếu đang chọn Door/Window: không thêm checkpoint mới như tường
            InsertDoorOrWindow(position, currentLineType);
            return; // Kết thúc luôn
        }

        // Kiểm tra nếu điểm mới gần p1, chỉ nối lại các điểm
        if (currentCheckpoints.Count > 2 && Vector3.Distance(currentCheckpoints[0].transform.position, position) < closeThreshold)
        {
            Vector3 start = currentCheckpoints[^1].transform.position;
            Vector3 end = currentCheckpoints[0].transform.position;

            DrawingTool.DrawLineAndDistance(start, end);

            // Tạo Room mới
            Room newRoom = new Room();

            // Lưu checkpoint
            foreach (GameObject cp in currentCheckpoints)
            {
                Vector3 pos = cp.transform.position;
                newRoom.checkpoints.Add(new Vector2(pos.x, pos.z));
            }

            // Tạo wallLines từ checkpoint
            for (int i = 0; i < currentCheckpoints.Count; i++)
            {
                Vector3 p1 = currentCheckpoints[i].transform.position;
                Vector3 p2 = (i == currentCheckpoints.Count - 1)
                    ? currentCheckpoints[0].transform.position
                    : currentCheckpoints[i + 1].transform.position;

                WallLine wall = new WallLine(p1, p2, currentLineType);
                newRoom.wallLines.Add(wall);
            }

            // Khi loop đóng
            // isClosedLoop = true;
            RoomStorage.rooms.Add(newRoom);
            Debug.Log("Đã lưu Room với " + newRoom.checkpoints.Count + " điểm và " + newRoom.wallLines.Count + " cạnh.");


            // === Ánh xạ List<GameObject> → Room.ID ===
            List<GameObject> loopRef = new List<GameObject>(currentCheckpoints);
            allCheckpoints.Add(loopRef);
            loopMappings.Add(new LoopMap(newRoom.ID, loopRef));

            currentCheckpoints.Clear();
            // isClosedLoop = true;
            // isClosedLoop = false;
            return;
        }

        GameObject checkpoint = Instantiate(checkpointPrefab, position, Quaternion.identity);
        // checkpoints.Add(checkpoint);
        currentCheckpoints.Add(checkpoint);

        // Nếu có ít nhất 2 điểm, nối chúng lại
        if (currentCheckpoints.Count > 1)
        {
            Vector3 start = currentCheckpoints[^2].transform.position;
            Vector3 end = checkpoint.transform.position;
            DrawingTool.DrawLineAndDistance(start, end);

            wallLines.Add(new WallLine(start, end, currentLineType));
        }
    }
    public void InsertDoorOrWindow(Vector3 clickPosition, LineType type)
    {
        if (firstDoorPoint == null)
        {
            // Lần bấm đầu tiên: tìm đoạn wall gần nhất
            float minDist = float.MaxValue;
            selectedRoomForDoor = null;
            selectedWallLineForDoor = null;

            foreach (Room room in RoomStorage.rooms)
            {
                foreach (var wl in room.wallLines)
                {
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
            // Hiển thị điểm P1 ngay lập tức
            GameObject p1Obj = Instantiate(checkpointPrefab, firstDoorPoint.Value, Quaternion.identity);
            p1Obj.name = $"{type}_P1_PREVIEW";

            Debug.Log($"Đã chọn P1: {firstDoorPoint}");
            return; // Chờ lần bấm thứ 2
        }
        else
        {
            // Lần bấm thứ 2: tính P2 và chèn
            Vector3 p1 = firstDoorPoint.Value;
            Vector3 projected = ProjectPointOnLineSegment(selectedWallLineForDoor.start, selectedWallLineForDoor.end, clickPosition);
            Vector3 p2 = projected;

            if (Vector3.Distance(p1, p2) < 0.01f)
            {
                Debug.LogWarning("P2 trùng P1, không hợp lệ.");
                return;
            }

            // Chia wallLine
            var newWalls = new List<WallLine>();
            foreach (var wl in selectedRoomForDoor.wallLines)
            {
                if ((ApproximatelyEqual(wl.start, selectedWallLineForDoor.start) && ApproximatelyEqual(wl.end, selectedWallLineForDoor.end)) ||
                    (ApproximatelyEqual(wl.start, selectedWallLineForDoor.end) && ApproximatelyEqual(wl.end, selectedWallLineForDoor.start)))
                {
                    // Chia 3 đoạn
                    newWalls.Add(new WallLine(wl.start, p1, LineType.Wall));
                    newWalls.Add(new WallLine(p1, p2, type));
                    newWalls.Add(new WallLine(p2, wl.end, LineType.Wall));
                }
                else
                {
                    newWalls.Add(wl);
                }
            }
            selectedRoomForDoor.wallLines = newWalls;

            // Update checkpoints
            int wallIndex = FindSegmentIndexInCheckpoint(selectedRoomForDoor.checkpoints, selectedWallLineForDoor.start, selectedWallLineForDoor.end);
            if (wallIndex != -1)
            {
                Vector2 vp1 = new Vector2(p1.x, p1.z);
                Vector2 vp2 = new Vector2(p2.x, p2.z);

                selectedRoomForDoor.checkpoints.Insert(wallIndex + 1, vp1);
                selectedRoomForDoor.checkpoints.Insert(wallIndex + 2, vp2);
            }

            GameObject p1Obj = Instantiate(checkpointPrefab, p1, Quaternion.identity);
            GameObject p2Obj = Instantiate(checkpointPrefab, p2, Quaternion.identity);
            p1Obj.name = $"{type}_P1";
            p2Obj.name = $"{type}_P2";

            Debug.Log($"Đã chèn {type} từ P1: {p1} đến P2: {p2}");

            firstDoorPoint = null; // reset
            selectedWallLineForDoor = null;
            selectedRoomForDoor = null;

            RedrawAllRooms();
        }
    }
    int FindSegmentIndexInCheckpoint(List<Vector2> points, Vector3 start, Vector3 end, float tolerance = 0.01f)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 a = new Vector3(points[i].x, 0, points[i].y);
            Vector3 b = new Vector3(points[(i + 1) % points.Count].x, 0, points[(i + 1) % points.Count].y);

            if ((Vector3.Distance(a, start) < tolerance && Vector3.Distance(b, end) < tolerance) ||
                (Vector3.Distance(a, end) < tolerance && Vector3.Distance(b, start) < tolerance))
            {
                return i;
            }
        }
        return -1;
    }
    Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
    void RedrawAllRooms()
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
    bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.001f)
    {
        return Vector3.Distance(a, b) < tolerance;
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

        if (nearestCheckpoint != null)
        {
            selectedCheckpoint = nearestCheckpoint;
            return true;
        }

        return false;
    }

    void MoveSelectedCheckpoint()
    {
        if (selectedCheckpoint == null) return;

        Vector3 newPosition = GetWorldPositionFromScreen(Input.mousePosition);
        selectedCheckpoint.transform.position = newPosition;

        foreach (var loop in allCheckpoints)
        {
            if (loop.Contains(selectedCheckpoint))
            {
                string roomID = FindRoomIDForLoop(loop);
                if (!string.IsNullOrEmpty(roomID))
                {
                    Room room = RoomStorage.rooms.Find(r => r.ID == roomID);
                    if (room != null)
                    {
                        for (int i = 0; i < loop.Count; i++)
                        {
                            Vector3 pos = loop[i].transform.position;
                            room.checkpoints[i] = new Vector2(pos.x, pos.z);
                        }

                        for (int i = 0; i < room.wallLines.Count; i++)
                        {
                            room.wallLines[i].start = new Vector3(room.checkpoints[i].x, 0, room.checkpoints[i].y);
                            room.wallLines[i].end = new Vector3(room.checkpoints[(i + 1) % room.checkpoints.Count].x, 0, room.checkpoints[(i + 1) % room.checkpoints.Count].y);
                        }

                        RoomStorage.UpdateOrAddRoom(room);
                    }
                }

                DrawingTool.ClearAllLines();
                RedrawAllRooms();
                break;
            }
        }
    }

    private string FindRoomIDForLoop(List<GameObject> loop)
    {
        foreach (var mapping in loopMappings)
        {
            if (ReferenceEquals(mapping.CheckpointsGO, loop)) return mapping.RoomID;
        }
        Debug.LogWarning("Loop không tìm thấy RoomID!");
        return null;
    }


    void DeselectCheckpoint()
    {
        selectedCheckpoint = null;
    }

    Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
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
            // 1) vẽ checkpoints
            var checkpointsForPath = new List<GameObject>();
            foreach (var pt in room.checkpoints)
            {
                var worldPos = new Vector3(pt.x, 0, pt.y);
                var cp = Instantiate(checkpointPrefab, worldPos, Quaternion.identity);
                checkpointsForPath.Add(cp);
            }
            // 2) vẽ outline
            for (int i = 1; i < checkpointsForPath.Count; i++)
                DrawingTool.DrawLineAndDistance(
                    checkpointsForPath[i - 1].transform.position,
                    checkpointsForPath[i].transform.position
                );
            // khép kín
            if (checkpointsForPath.Count > 2)
                DrawingTool.DrawLineAndDistance(
                    checkpointsForPath[^1].transform.position,
                    checkpointsForPath[0].transform.position
                );
            // 3) vẽ các WallLine (bao gồm tường thường, cửa, cửa sổ)
            foreach (var wall in room.wallLines)
                DrawingTool.DrawLineAndDistance(wall.start, wall.end);
        }
    }

    void ShowIncompleteLoopPopup()
    {
        PopupController.Show(
            "Mạch chưa khép kín!\nBạn muốn xóa dữ liệu vẽ tạm không?",
            onYes: () =>
            {
                Debug.Log("Người dùng chọn YES: Xóa toàn bộ checkpoint + line.");
                DeleteCurrentDrawingData();
            },
            onNo: () =>
            {
                Debug.Log("Người dùng chọn NO: Tiếp tục vẽ để khép kín.");
            }
        );
    }
    void DeleteCurrentDrawingData()
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

        Debug.Log("Đã xóa toàn bộ dữ liệu vẽ chưa khép kín.");
    }
}
