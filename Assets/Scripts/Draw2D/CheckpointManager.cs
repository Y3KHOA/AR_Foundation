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

    void Start()
    {
        // LoadPointsFromDataTransfer();
        LoadPointsFromRoomStorage();
    }

    void Update()
    {
        if (!PenManager.isPenActive)
        {
            // Khi Pen không hoạt động, không cho phép đặt điểm và chỉ di chuyển camera
            penManager.HandleZoomAndPan(true);  // Bật zoom và di chuyển camera
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
            if (selectedCheckpoint != null && isClosedLoop) // Nếu đã chọn điểm và mạch kín, di chuyển điểm
            {
                // MoveSelectedCheckpoint();
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

    void SelectCheckpoint()
    {
        Vector3 clickPosition = GetWorldPositionFromScreen(Input.mousePosition);
        TrySelectCheckpoint(clickPosition);
    }

    void HandleCheckpointPlacement(Vector3 position)
    {
        if (selectedCheckpoint != null) return; // Nếu đã chọn điểm, không cần đặt mới

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

            RoomStorage.rooms.Add(newRoom);
            Debug.Log("Đã lưu Room với " + newRoom.checkpoints.Count + " điểm và " + newRoom.wallLines.Count + " cạnh.");

            allCheckpoints.Add(new List<GameObject>(currentCheckpoints));
            currentCheckpoints.Clear();
            isClosedLoop = true;
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

    bool TrySelectCheckpoint(Vector3 position)
    {
        // if (!isClosedLoop) return false;
        if (!isClosedLoop && currentCheckpoints.Count > 0)
        {
            allCheckpoints.Add(new List<GameObject>(currentCheckpoints));
        }

        float minDistance = closeThreshold;
        GameObject nearestCheckpoint = null;

        foreach (var checkpoint in currentCheckpoints)
        {
            float distance = Vector3.Distance(checkpoint.transform.position, position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCheckpoint = checkpoint;
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
        if (selectedCheckpoint == null || !isClosedLoop) return;

        Vector3 newPosition = GetWorldPositionFromScreen(Input.mousePosition);
        selectedCheckpoint.transform.position = newPosition;
        DrawingTool.UpdateLinesAndDistances(currentCheckpoints);
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

    void LoadPointsFromDataTransfer()
    {
        List<List<Vector2>> allPoints = DataTransfer.Instance.GetAllPoints();
        List<List<float>> allHeights = DataTransfer.Instance.GetAllHeights();
        List<List<WallLine>> allWallLines = DataTransfer.Instance.GetAllWallLines(); // thêm WallLines

        if (allPoints.Count == 0)
        {
            Debug.Log("Không có dữ liệu điểm để hiển thị.");
            return;
        }

        for (int pathIndex = 0; pathIndex < allPoints.Count; pathIndex++)
        {
            List<Vector2> path = allPoints[pathIndex];
            List<GameObject> checkpointsForPath = new List<GameObject>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = new Vector3(path[i].x, 0, path[i].y);
                GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity);
                checkpointsForPath.Add(checkpoint);

                if (i > 0)
                {
                    DrawingTool.DrawLineAndDistance(checkpointsForPath[i - 1].transform.position, worldPos);
                }
            }

            if (checkpointsForPath.Count > 2 && Vector3.Distance(checkpointsForPath[0].transform.position, checkpointsForPath[^1].transform.position) < closeThreshold)
            {
                DrawingTool.DrawLineAndDistance(checkpointsForPath[^1].transform.position, checkpointsForPath[0].transform.position);
                isClosedLoop = true;
            }

            allCheckpoints.Add(checkpointsForPath);

            // Vẽ thêm WallLines của Room này
            if (pathIndex < allWallLines.Count)
            {
                foreach (WallLine wall in allWallLines[pathIndex])
                {
                    DrawingTool.DrawLineAndDistance(wall.start, wall.end);
                }
            }
        }

        Debug.Log($"[LoadPoints] Đã nạp {allPoints.Count} mạch với tổng cộng {CountTotalCheckpoints()} checkpoint và WallLines.");
    }

    int CountTotalCheckpoints()
    {
        int total = 0;
        foreach (var list in allCheckpoints)
            total += list.Count;
        return total;
    }

    public bool TryFindClosestSegment(Vector3 position, out int loopIndex, out int segmentIndex, float maxDistance = 0.1f)
    {
        loopIndex = -1;
        segmentIndex = -1;
        float closestDist = maxDistance;

        for (int i = 0; i < allCheckpoints.Count; i++)
        {
            var loop = allCheckpoints[i];
            for (int j = 0; j < loop.Count; j++)
            {
                int next = (j + 1) % loop.Count;
                Vector3 a = loop[j].transform.position;
                Vector3 b = loop[next].transform.position;

                float dist = DistanceFromPointToSegment(position, a, b);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    loopIndex = i;
                    segmentIndex = j;
                }
            }
        }

        return loopIndex != -1;
    }
    public float DistanceFromPointToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;
        float magnitudeAB = ab.sqrMagnitude;
        float abDotAp = Vector3.Dot(ap, ab);
        float t = Mathf.Clamp01(abDotAp / magnitudeAB);
        Vector3 projection = a + ab * t;
        return Vector3.Distance(p, projection);
    }
    private void InsertCheckpointIntoExistingLoop(Vector3 position)
    {
        if (TryFindClosestSegment(position, out int loopIndex, out int segmentIndex))
        {
            GameObject newCheckpoint = Instantiate(checkpointPrefab, position, Quaternion.identity);
            var loop = allCheckpoints[loopIndex];

            loop.Insert(segmentIndex + 1, newCheckpoint);// Chèn vào sau segmentIndex
            DrawingTool.UpdateLinesAndDistances(loop); // Vẽ lại vòng đó

            Debug.Log($"Chèn checkpoint vào vòng {loopIndex} sau đoạn {segmentIndex}");
        }
    }
}
