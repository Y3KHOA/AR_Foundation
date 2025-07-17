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

    [Header("Camera")]
    public Camera drawingCamera; // Gán Camera chính vẽ 2D

    public bool isMovingCheckpoint = false;
    public List<GameObject> currentCheckpoints = new List<GameObject>();
    public GameObject selectedCheckpoint = null; // Điểm được chọn để di chuyển  
    public bool isDragging = false; // Kiểm tra xem có đang kéo điểm không 
    public bool isPreviewing = false; // Trạng thái preview
    public bool isClosedLoop = false; // Biến kiểm tra xem mạch đã khép kín chưa 
    public List<List<GameObject>> AllCheckpoints => allCheckpoints; // Truy cập danh sách tất cả các checkpoint từ bên ngoài
    public bool IsDraggingRoom = false;
    public GameObject previewCheckpoint = null;

    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();
    private float closeThreshold = 0.5f; // Khoảng cách tối đa để chọn điểm
    private Vector3 previewPosition; // Vị trí preview

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
        else if (Input.GetMouseButtonUp(0))
        {
            isPreviewing = false;
            DrawingTool.ClearPreviewLine();

            if (previewCheckpoint != null)
            {
                Destroy(previewCheckpoint);
            }

            HandleCheckpointPlacement(previewPosition); // Vẽ checkpoint mới

            DeselectCheckpoint(); // Không thật sự cần, nhưng không hại
            isDragging = false;
        }
    }

    public bool IsInSavedLoop(GameObject checkpoint)
    {
        foreach (var loop in allCheckpoints)
        {
            if (loop.Contains(checkpoint))
                return true;
        }
        return false;
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
                pos.y = 0f;
                newRoom.checkpoints.Add(new Vector2(pos.x, pos.z));
            }
            // === Fix winding: đảo nếu diện tích âm
            if (MeshGenerator.CalculateArea(newRoom.checkpoints) > 0)
            {
                newRoom.checkpoints.Reverse();
                Debug.Log("Đã đảo chiều polygon để mesh đúng mặt.");
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

            // === Sinh mesh sàn tự động ===
            GameObject floorGO = new GameObject($"RoomFloor_{newRoom.ID}");
            floorGO.transform.parent = null;
            floorGO.transform.position = Vector3.zero;
            floorGO.transform.rotation = Quaternion.identity;
            floorGO.transform.localScale = Vector3.one;
            RoomMeshController meshCtrl = floorGO.AddComponent<RoomMeshController>();
            meshCtrl.Initialize(newRoom.ID);

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
            // Lưu roomID trước
            string roomID = selectedRoomForDoor.ID;

            firstDoorPoint = null; // reset
            selectedWallLineForDoor = null;
            selectedRoomForDoor = null;

            RedrawAllRooms();

            // === Update Floor Mesh ===
            var floorGO = GameObject.Find($"RoomFloor_{roomID}");
            if (floorGO != null)
            {
                var meshCtrl = floorGO.GetComponent<RoomMeshController>();
                if (meshCtrl != null)
                {
                    // meshCtrl.GenerateMesh(selectedRoomForDoor.checkpoints);
                    meshCtrl.GenerateMesh(RoomStorage.GetRoomByID(roomID).checkpoints);
                }
            }
            else
            {
                Debug.LogWarning($"RoomFloor_{selectedRoomForDoor.ID} không tồn tại!");
            }
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

    public void MoveSelectedCheckpoint()
    {
        if (selectedCheckpoint == null) return;

        Vector3 newPosition = GetWorldPositionFromScreen(Input.mousePosition);
        selectedCheckpoint.transform.position = newPosition;

        isMovingCheckpoint = true; // Đánh dấu đang move

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
                        // Cập nhật checkpoints
                        for (int i = 0; i < loop.Count; i++)
                        {
                            Vector3 pos = loop[i].transform.position;
                            room.checkpoints[i] = new Vector2(pos.x, pos.z);
                        }

                        // Cập nhật wallLines
                        for (int i = 0; i < room.wallLines.Count; i++)
                        {
                            room.wallLines[i].start = new Vector3(room.checkpoints[i].x, 0, room.checkpoints[i].y);
                            room.wallLines[i].end = new Vector3(room.checkpoints[(i + 1) % room.checkpoints.Count].x, 0, room.checkpoints[(i + 1) % room.checkpoints.Count].y);
                        }

                        RoomStorage.UpdateOrAddRoom(room);

                        // Update Mesh sàn
                        var floorGO = GameObject.Find($"RoomFloor_{roomID}");
                        if (floorGO != null)
                        {
                            var meshCtrl = floorGO.GetComponent<RoomMeshController>();
                            if (meshCtrl != null)
                            {
                                meshCtrl.GenerateMesh(room.checkpoints);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Không tìm thấy RoomFloor GameObject với ID: {roomID}");
                        }
                    }
                }

                DrawingTool.ClearAllLines();
                RedrawAllRooms();
                break;
            }
        }
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

        Debug.Log("Đã xóa toàn bộ dữ liệu vẽ chưa khép kín.");
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

        // 1) Xoá dữ liệu tạm nếu đang vẽ dở
        // DeleteCurrentDrawingData();

        // 2) Tìm center trên mặt phẳng y=0 theo camera
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

        // 3) Tính bán kính từ chiều dài cạnh
        float radius = edgeLength / (2 * Mathf.Sin(Mathf.PI / sides));
        Debug.Log($"Bán kính: {radius}");

        // 4) Tạo các checkpoint prefab
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

        // 5) Tạo wallLines & vẽ line
        for (int i = 0; i < currentCheckpoints.Count; i++)
        {
            Vector3 p1 = currentCheckpoints[i].transform.position;
            Vector3 p2 = (i == currentCheckpoints.Count - 1)
                ? currentCheckpoints[0].transform.position
                : currentCheckpoints[i + 1].transform.position;

            DrawingTool.DrawLineAndDistance(p1, p2);
            wallLines.Add(new WallLine(p1, p2, LineType.Wall));
        }

        // 6) Tạo Room & lưu
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

        // 1) Xoá dữ liệu tạm nếu đang vẽ dở
        // DeleteCurrentDrawingData();

        // 2) Tìm center trên mặt phẳng y=0 theo camera
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

        // 3) Tính 4 đỉnh hình chữ nhật quanh center
        Vector3 p1 = new Vector3(center.x - width / 2, 0, center.z - height / 2);
        Vector3 p2 = new Vector3(center.x - width / 2, 0, center.z + height / 2);
        Vector3 p3 = new Vector3(center.x + width / 2, 0, center.z + height / 2);
        Vector3 p4 = new Vector3(center.x + width / 2, 0, center.z - height / 2);

        List<Vector3> corners = new List<Vector3> { p1, p2, p3, p4 };

        // 4) Tạo checkpoint prefab tại từng góc
        foreach (Vector3 pos in corners)
        {
            var cp = Instantiate(checkpointPrefab, pos, Quaternion.identity);
            currentCheckpoints.Add(cp);
        }

        // 5) Tạo wallLines & vẽ line
        for (int i = 0; i < currentCheckpoints.Count; i++)
        {
            Vector3 start = currentCheckpoints[i].transform.position;
            Vector3 end = (i == currentCheckpoints.Count - 1)
                ? currentCheckpoints[0].transform.position
                : currentCheckpoints[i + 1].transform.position;

            DrawingTool.DrawLineAndDistance(start, end);
            wallLines.Add(new WallLine(start, end, LineType.Wall));
        }

        // 6) Tạo Room & lưu
        Room newRoom = new Room();
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

        // 7) Tạo mesh sàn
        GameObject floorGO = new GameObject($"RoomFloor_{newRoom.ID}");
        RoomMeshController meshCtrl = floorGO.AddComponent<RoomMeshController>();
        meshCtrl.Initialize(newRoom.ID);

        // 8) Ánh xạ loop
        List<GameObject> loopRef = new List<GameObject>(currentCheckpoints);
        allCheckpoints.Add(loopRef);
        loopMappings.Add(new LoopMap(newRoom.ID, loopRef));

        currentCheckpoints.Clear();
        wallLines.Clear();

        Debug.Log($"Đã tạo Room hình chữ nhật: {width} x {height} m, RoomID: {newRoom.ID}");
    }
}
