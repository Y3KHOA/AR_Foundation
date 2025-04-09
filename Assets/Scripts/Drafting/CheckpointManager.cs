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

    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();
    private List<GameObject> currentCheckpoints = new List<GameObject>();
    private GameObject selectedCheckpoint = null; // Điểm được chọn để di chuyển
    private float closeThreshold = 0.5f; // Khoảng cách tối đa để chọn điểm
    private bool isDragging = false; // Kiểm tra xem có đang kéo điểm không
    private Vector3 previewPosition; // Vị trí preview
    private bool isPreviewing = false; // Trạng thái preview
    private bool isClosedLoop = false; // Biến kiểm tra xem mạch đã khép kín chưa
    private GameObject previewCheckpoint = null;

    void Start()
    {
        LoadPointsFromDataTransfer();
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

            if (!isDragging) // Nếu không phải kéo điểm, đặt checkpoint mới
            {
                HandleCheckpointPlacement(previewPosition);
            }

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
            DrawingTool.DrawLineAndDistance(currentCheckpoints[^1].transform.position, currentCheckpoints[0].transform.position);
            isClosedLoop = true;

            allCheckpoints.Add(new List<GameObject>(currentCheckpoints)); // Lưu mạch cũ
            currentCheckpoints.Clear(); // Tạo mạch mới
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

    void LoadPointsFromDataTransfer()
    {
        List<List<Vector2>> allPoints = DataTransfer.Instance.GetAllPoints();
        List<List<float>> allHeights = DataTransfer.Instance.GetAllHeights();

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
                Vector3 worldPos = new Vector3(path[i].x, 0, path[i].y); // Y = 0 vì hiển thị 2D
                GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity);
                checkpointsForPath.Add(checkpoint);

                // Nếu có ít nhất 2 điểm, vẽ line giữa các điểm
                if (i > 0)
                {
                    DrawingTool.DrawLineAndDistance(checkpointsForPath[i - 1].transform.position, worldPos);
                }
            }

            // Tự động nối kín nếu đủ điểm và 2 đầu gần nhau
            if (checkpointsForPath.Count > 2 && Vector3.Distance(checkpointsForPath[0].transform.position, checkpointsForPath[^1].transform.position) < closeThreshold)
            {
                DrawingTool.DrawLineAndDistance(checkpointsForPath[^1].transform.position, checkpointsForPath[0].transform.position);
                isClosedLoop = true;
            }

            // Lưu vào list tổng
            allCheckpoints.Add(checkpointsForPath);
        }

        Debug.Log($"[LoadPoints] Đã nạp {allPoints.Count} mạch với tổng cộng {CountTotalCheckpoints()} checkpoint.");
    }

    int CountTotalCheckpoints()
    {
        int total = 0;
        foreach (var list in allCheckpoints)
            total += list.Count;
        return total;
    }


    public void ExportAllDrawingsToPDF()
    {
        List<List<Vector2>> allPolygons = new List<List<Vector2>>();
        List<List<float>> allDistances = new List<List<float>>();

        foreach (var checkpointLoop in allCheckpoints)
        {
            if (checkpointLoop == null || checkpointLoop.Count < 2)
                continue;

            List<Vector2> polygon = new List<Vector2>();
            List<float> distances = new List<float>();

            for (int i = 0; i < checkpointLoop.Count; i++)
            {
                Vector3 pos = checkpointLoop[i].transform.position;
                polygon.Add(new Vector2(pos.x, pos.z));

                if (i > 0)
                {
                    Vector3 prev = checkpointLoop[i - 1].transform.position;
                    distances.Add(Vector3.Distance(prev, pos));
                }
            }

            // Xử lý: nếu polygon khép kín và điểm cuối == điểm đầu → loại bỏ điểm cuối
            if (polygon.Count > 2 && Vector2.Distance(polygon[0], polygon[^1]) < 0.01f)
            {
                polygon.RemoveAt(polygon.Count - 1);

                // Nếu distances dư 1 phần tử thì cũng cần xóa
                if (distances.Count == polygon.Count + 1)
                    distances.RemoveAt(distances.Count - 1);
            }

            allPolygons.Add(polygon);
            allDistances.Add(distances);
        }

        string path = Path.Combine(Application.persistentDataPath, "Drawing_All_Test1.pdf");

#if UNITY_ANDROID && !UNITY_EDITOR
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#endif
        // #if UNITY_ANDROID && !UNITY_EDITOR
        //     if (!permissionRequester.IsAllFilesAccessGranted())
        //     {
        //         Debug.LogWarning("chua co quyen All Files Access. dang yeu cau...");
        //         permissionRequester.RequestAllFilesAccessWithPopup();
        //         return;
        //     }
        // #endif

        // #if UNITY_ANDROID && !UNITY_EDITOR
        //     string folderPath = "/storage/emulated/0/XHeroScan"; // thư mục dễ tìm gần thư mục Download
        // #else
        //         string folderPath = Path.Combine(Application.persistentDataPath, "XHeroScan");
        // #endif
        //         string pdfFileName = "Drawing_All_Test1.pdf";
        //         if (!Directory.Exists(folderPath))
        //             Directory.CreateDirectory(folderPath);

        // string path = Path.Combine(folderPath, pdfFileName);

        PdfExporter.ExportMultiplePolygonsToPDF(allPolygons, allDistances, path, "m");

        Debug.Log("PDF exported to: " + path);
    }
}
