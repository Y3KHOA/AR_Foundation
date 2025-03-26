using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARMeasureTool : MonoBehaviour
{
    [Header("Prefabs can thiet")]
    public GameObject checkpointPrefab;
    public GameObject distanceTextPrefab;
    public GameObject linePrefab;

    [Header("Cai dat do luong")]
    public float closeThreshold = 0.2f; // 0.2m = 20cm

    public ARRaycastManager raycastManager; // Public de gan trong Unity

    private List<Vector3> points = new List<Vector3>();
    private List<GameObject> checkpoints = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> distanceTexts = new List<GameObject>();


    private GameObject selectedCheckpoint = null; // Lưu checkpoint đang chọn
    private float initialPinchDistance = 0f; // Khoảng cách ban đầu khi pinch zoom
    float minScale = 0.05f; // Giới hạn nhỏ nhất
    float maxScale = 2.0f;  // Giới hạn lớn nhất

    void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                List<ARRaycastHit> hits = new List<ARRaycastHit>();

                if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    PlaceCheckpoint(hitPose.position);
                }
            }
        }

        if (Input.touchCount == 1) // Một ngón tay: Chọn hoặc di chuyển checkpoint
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                SelectCheckpoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && selectedCheckpoint != null)
            {
                MoveSelectedCheckpoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                DeselectCheckpoint();
            }
        }
        else if (Input.touchCount == 2 && selectedCheckpoint != null) // Hai ngón tay: Phóng to/thu nhỏ checkpoint
        {
            HandlePinchToResize();
        }
    }


    List<List<Vector3>> allPolygons = new List<List<Vector3>>(); // Lưu tất cả các vùng đo

    void SelectCheckpoint(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Tìm checkpoint gần nhất với điểm chạm
            float minDistance = float.MaxValue;
            GameObject nearestCheckpoint = null;

            foreach (var checkpoint in checkpoints)
            {
                float distance = Vector3.Distance(checkpoint.transform.position, hitPose.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCheckpoint = checkpoint;
                }
            }

            if (nearestCheckpoint != null)
            {
                selectedCheckpoint = nearestCheckpoint;
                // Tang scale cua checkpoint len 0.1 (tuc la cong them Vector3.one * 0.1)
                selectedCheckpoint.transform.localScale += Vector3.one * 0.1f;
                Debug.Log("Da chon checkpoint.");
            }
        }
    }

    void MoveSelectedCheckpoint(Vector2 touchPosition)
    {
        if (selectedCheckpoint == null) return; // Không làm gì nếu chưa chọn checkpoint

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            selectedCheckpoint.transform.position = hitPose.position; // Di chuyển checkpoint đến vị trí mới
            Debug.Log("Di chuyển checkpoint.");

            UpdateLinesAndDistances(); // Cập nhật lại đường nối
        }
    }
    void UpdateLinesAndDistances()
    {
        // Cập nhật lại các đường nối
        for (int i = 0; i < lines.Count; i++)
        {
            LineRenderer line = lines[i].GetComponent<LineRenderer>();
            if (line != null)
            {
                // Cập nhật vị trí cho các checkpoint (nối từ checkpoint[i] đến checkpoint[(i+1)%n])
                line.SetPosition(0, checkpoints[i].transform.position);
                line.SetPosition(1, checkpoints[(i + 1) % checkpoints.Count].transform.position);
            }

            // Tính lại khoảng cách giữa hai checkpoint tương ứng
            float distanceInMeters = Vector3.Distance(checkpoints[i].transform.position, checkpoints[(i + 1) % checkpoints.Count].transform.position);
            float distanceInCm = distanceInMeters * 100f;

            // Nếu có distanceText tương ứng thì cập nhật text
            if (i < distanceTexts.Count)
            {
                TextMeshPro tmp = distanceTexts[i].GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = $"{distanceInCm:F1} cm";
                    Debug.Log($"canh {i + 1} moi: {distanceInCm:F1} cm");
                }
            }
        }
    }


    void HandlePinchToResize()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        if (initialPinchDistance == 0f)
        {
            initialPinchDistance = currentDistance;
            return;
        }

        float scaleMultiplier = currentDistance / initialPinchDistance;
        Vector3 newScale = selectedCheckpoint.transform.localScale * scaleMultiplier;

        // Giới hạn kích thước checkpoint
        newScale = Vector3.Max(Vector3.one * minScale, Vector3.Min(newScale, Vector3.one * maxScale));

        selectedCheckpoint.transform.localScale = newScale;
        initialPinchDistance = currentDistance;
        Debug.Log("Thay doi kich thuoc checkpoint.");
    }
    void DeselectCheckpoint()
    {
        if (selectedCheckpoint != null)
        {
            // Giam scale ve ban dau (loai bo phan cong them)
            selectedCheckpoint.transform.localScale -= Vector3.one * 0.1f;
        }
        selectedCheckpoint = null;
        initialPinchDistance = 0f;
    }
    void PlaceCheckpoint(Vector3 newPoint)
    {
        if (checkpointPrefab == null)
        {
            Debug.LogError("checkpointPrefab chưa được gán!");
            return;
        }

        // Nếu điểm mới gần điểm đầu tiên (p1), thì khép vùng và kết thúc
        if (points.Count > 2 && Vector3.Distance(newPoint, points[0]) < closeThreshold)
        {
            DrawLineAndDistance(points[points.Count - 1], points[0]); // Nối về điểm đầu
            ShowAreaText(points[0], CalculateArea(points)); // Hiển thị diện tích
            Debug.Log($"Dien tich vung: {CalculateArea(points):F2} m2");

            // Lưu vùng đo hiện tại vào danh sách
            allPolygons.Add(new List<Vector3>(points));

            // Bắt đầu một vùng đo mới
            points.Clear();
            checkpoints.Clear();  // Đảm bảo checkpoints reset đúng
            return;
        }

        // Nếu không phải điểm đóng vùng, thì tiếp tục thêm checkpoint
        GameObject checkpoint = Instantiate(checkpointPrefab, newPoint, Quaternion.identity);
        checkpoints.Add(checkpoint);
        points.Add(newPoint);

        if (checkpoint.GetComponent<Collider>() == null)
            checkpoint.AddComponent<SphereCollider>();

        Debug.Log($"Them checkpoint {checkpoints.Count} tại {newPoint}");

        // Vẽ đường từ điểm trước đó đến điểm mới
        if (points.Count > 1)
        {
            DrawLineAndDistance(points[points.Count - 2], newPoint);
        }
    }




    void ShowAreaText(Vector3 position, float area)
    {
        if (distanceTextPrefab != null)
        {
            GameObject textObj = Instantiate(distanceTextPrefab, position, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                textMesh.text = $"Dien tich: {area:F2} m2";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogError("distanceTextPrefab khong co TextMeshPro!");
            }
        }
    }


    void DrawLineAndDistance(Vector3 start, Vector3 end)
    {
        int edgeNumber = lines.Count + 1; // Đánh số thứ tự cạnh
        if (linePrefab != null)
        {
            GameObject lineObj = Instantiate(linePrefab);
            LineRenderer line = lineObj.GetComponent<LineRenderer>();

            if (line != null)
            {
                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                lines.Add(lineObj);
            }
            else
            {
                Debug.LogError("linePrefab khong co LineRenderer!");
            }
        }
        //Tinh khoan cach và in ra log
        float distanceInMeters = Vector3.Distance(start, end);
        float distanceInCm = distanceInMeters * 100f;

        Debug.Log($"Canh{edgeNumber}: {distanceInCm:F1} cm");

        //hiển thị khoản cách giữa 2 điểm tính theo cm
        Vector3 midPoint = (start + end) / 2;
        if (distanceTextPrefab != null)
        {
            GameObject textObj = Instantiate(distanceTextPrefab, midPoint, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                textMesh.text = $"{distanceInCm:F1} cm";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogError("distanceTextPrefab khong co TextMeshPro!");
            }
            distanceTexts.Add(textObj);
        }
    }

    float CalculateArea(List<Vector3> points)
    {
        if (points.Count < 3) return 0f;

        float perimeter = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            float edgeLength = Vector3.Distance(points[i], points[i + 1]);
            perimeter += edgeLength;
        }
        perimeter += Vector3.Distance(points[points.Count - 1], points[0]); // Nối điểm cuối với điểm đầu

        float estimatedArea = (perimeter * perimeter) / (4 * Mathf.PI);
        Debug.Log($"Chu vi da giac: {perimeter:F2} m");
        Debug.Log($"Dien tich uoc luong: {estimatedArea:F2} m2");

        return estimatedArea;
    }

    public void ResetMeasurement()
    {
        points.Clear();
        foreach (var obj in checkpoints) Destroy(obj);
        checkpoints.Clear();
        foreach (var obj in lines) Destroy(obj);
        lines.Clear();
        foreach (var obj in distanceTexts) Destroy(obj);
        distanceTexts.Clear();

        Debug.Log("Reset do luong hoan tat!");
    }
}
