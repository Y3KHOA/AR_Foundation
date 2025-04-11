using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class BtnController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button toggleButton;

    [Header("AR Elements")]
    public GameObject pointPrefab;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public LineManager lineManager;
    public GameObject distanceTextPrefab;
    public float heightValue = 0.5f;

    private GameObject previewPoint = null;  // Điểm xem trước
    private GameObject spawnedPoint;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<List<GameObject>> allBasePoints = new List<List<GameObject>>();
    private List<List<GameObject>> allHeightPoints = new List<List<GameObject>>();
    private List<GameObject> currentBasePoints = new List<GameObject>();
    private List<GameObject> currentHeightPoints = new List<GameObject>();
    private GameObject referenceHeightPoint = null;

    private bool isPointVisible = false;
    private bool hasPlane = false;
    private float mergeThreshold = 0.1f; // Ngưỡng hợp nhất điểm
    private float closeThreshold = 0.2f; // Ngưỡng khép kín đường
    private int flag = 0; // Mặc định flag = 0
    public int Flag { get { return flag; } }
    private bool measure = true;
    private Vector3 fixedBasePointPosition;
    private float initialCameraPitch;
    private GameObject tempBasePoint;

    void Start()
    {
        // Lấy đơn vị đo từ bộ nhớ
        string unit = PlayerPrefs.GetString("SelectedUnit", "m");
        float savedHeight = PlayerPrefs.GetFloat("HeightValue", 0f);
        this.heightValue = savedHeight;

        if (btnByCam.Instance.IsMeasure)
        {
            heightValue = 0f;
            Debug.Log("btn da vao day heightValue: " + heightValue);

            btnByCam.Instance.IsMeasure = false;
        }

        Debug.Log("[Unity]Chieu cao nhan duoc: " + heightValue);

        if (toggleButton == null)
            Debug.LogError("toggleButton isEmpty!");

        toggleButton.onClick.AddListener(HandleButtonClick);
        toggleButton.onClick.AddListener(TogglePointVisibility);

        if (planeManager != null)
            planeManager.planesChanged += OnPlanesChanged;
    }

    void Update()
    {
        if (!hasPlane || !isPointVisible || pointPrefab == null || raycastManager == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (measure)
        {

            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (spawnedPoint == null)
                {
                    spawnedPoint = Instantiate(pointPrefab, hitPose.position, Quaternion.identity);
                }
                else
                {
                    spawnedPoint.transform.position = hitPose.position;
                }

                // Đảm bảo previewPoint tồn tại
                if (previewPoint == null)
                {
                    previewPoint = Instantiate(pointPrefab, hitPose.position, Quaternion.identity);
                    previewPoint.name = "PreviewPoint";
                }

                // Cập nhật vị trí preview point
                previewPoint.transform.position = hitPose.position;

                // Chỉ vẽ preview line nếu có ít nhất 1 điểm cơ sở
                if (currentBasePoints.Count > 0)
                {
                    Vector3 lastBasePoint = currentBasePoints[currentBasePoints.Count - 1].transform.position;
                    Vector3 previewPos = previewPoint.transform.position;

                    Debug.Log($"[Update] Draw PreviewLine from {lastBasePoint} to {previewPos}");
                    lineManager.DrawPreviewLine(lastBasePoint, previewPos);
                }
            }
        }
        else
        {
            // Lấy camera
            Camera cam = Camera.main != null ? Camera.main : Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null;
            if (cam == null) return;

            float currentPitch = cam.transform.eulerAngles.x;

            if (previewPoint == null)
            {
                // Đặt previewPoint tại vị trí base theo trục Y
                previewPoint = Instantiate(pointPrefab, fixedBasePointPosition + Vector3.up * 0.01f, Quaternion.identity);
                previewPoint.name = "HeightPreview";

                // Gán pitch ban đầu
                initialCameraPitch = currentPitch;
            }

            // Tính độ lệch góc quay
            float deltaPitch = Mathf.DeltaAngle(initialCameraPitch, currentPitch);

            // Tính khoảng cách từ camera đến điểm base
            float distanceToBase = Vector3.Distance(cam.transform.position, fixedBasePointPosition);

            // Dựa vào khoảng cách để scale (nếu gần thì scale nhỏ, xa thì scale lớn)
            float pitchToHeightScale = distanceToBase * 0.02f; // Có thể tinh chỉnh hệ số này

            // Tính Y mới và clamp không cho nhỏ hơn base
            float rawY = fixedBasePointPosition.y - deltaPitch * pitchToHeightScale;
            float newY = Mathf.Max(fixedBasePointPosition.y, rawY); // không cho thấp hơn điểm base

            // Cập nhật vị trí previewPoint
            previewPoint.transform.position = new Vector3(fixedBasePointPosition.x, newY, fixedBasePointPosition.z);

            Debug.Log("measure = false btn da vao day heightValue = " + newY);

            // Vẽ line theo trục Y giữa điểm gốc và preview
            lineManager.DrawPreviewLine(fixedBasePointPosition, previewPoint.transform.position);
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (planeManager.trackables.count > 0)
        {
            hasPlane = true;
            if (!isPointVisible)
                isPointVisible = true;
        }
    }

    void TogglePointVisibility()
    {
        isPointVisible = !isPointVisible;
        if (!isPointVisible && spawnedPoint != null)
        {
            Destroy(spawnedPoint);
        }
    }

    /// <summary>
    /// Hàm xử lý tổng khi nhấn create button 
    /// </summary>
    void HandleButtonClick()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        Pose hitPose = hits[0].pose;

        if (heightValue == 0)
        {
            Debug.Log("1 btn co vao day ko? heightValue = 0");
            if (measure)
            {
                Debug.Log("2 btn co vao day ko? measure = false");
                measure = false;

                // Tạo base point và lưu lại tạm
                tempBasePoint = GetOrCreatePoint(currentBasePoints, hitPose.position);

                fixedBasePointPosition = tempBasePoint.transform.position;

                // Reset lại camera pitch để chuẩn bị đo mới
                Camera cam = Camera.main != null ? Camera.main : Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null;
                if (cam != null)
                {
                    initialCameraPitch = cam.transform.eulerAngles.x;
                }

                return;
            }
            else
            {
                // Lần nhấn thứ hai - Kết thúc đo chiều cao
                Debug.Log("Lần nhấn 2 - Lưu chiều cao");

                if (previewPoint != null)
                {
                    heightValue = previewPoint.transform.position.y - fixedBasePointPosition.y;
                    heightValue = Mathf.Max(0, heightValue); // Đảm bảo không âm
                    Debug.Log("heightValue = " + heightValue);

                    lineManager.DestroyPreviewObjects();
                    previewPoint.SetActive(false);
                    Destroy(previewPoint); previewPoint = null;

                    // Xóa tempBasePoint nếu có
                    if (tempBasePoint != null)
                    {
                        if (currentBasePoints.Contains(tempBasePoint))
                        {
                            currentBasePoints.Remove(tempBasePoint);
                        }
                        Destroy(tempBasePoint);
                        tempBasePoint = null;
                    }
                }

                /// Reset lại trạng thái
                measure = true;
                fixedBasePointPosition = Vector3.zero;
                initialCameraPitch = 0f;

                return;
            }
        }

        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Debug.Log("Không tìm thấy mặt phẳng để đặt object.");
            return;
        }

        GameObject newBasePoint = GetOrCreatePoint(currentBasePoints, hitPose.position);
        GameObject newHeightPoint = referenceHeightPoint != null
            ? GetOrCreatePoint(currentHeightPoints, new Vector3(hitPose.position.x, referenceHeightPoint.transform.position.y, hitPose.position.z))
            : GetOrCreatePoint(currentHeightPoints, hitPose.position + new Vector3(0, heightValue, 0));

        if (referenceHeightPoint == null)
            referenceHeightPoint = newHeightPoint;

        currentBasePoints.Add(newBasePoint);
        currentHeightPoints.Add(newHeightPoint);

        int count = currentBasePoints.Count;

        // Tự động nối Pn với Pn-1
        if (count > 1)
        {
            lineManager.DrawLineAndDistance(currentBasePoints[count - 2].transform.position, newBasePoint.transform.position);
            lineManager.DrawLineAndDistance(currentHeightPoints[count - 2].transform.position, newHeightPoint.transform.position);
        }

        // Kiểm tra nếu Pn gần P1, tự động khép kín đường
        if (count > 2 && Vector3.Distance(newBasePoint.transform.position, currentBasePoints[0].transform.position) < closeThreshold)
        {
            lineManager.DrawLineAndDistance(newBasePoint.transform.position, currentBasePoints[0].transform.position);
            lineManager.DrawLineAndDistance(newHeightPoint.transform.position, currentHeightPoints[0].transform.position);
            flag = 1; // Đánh dấu đã khép kín đường

            // Tính diện tích giữa các mặt đáy và mặt trên
            float baseArea = AreaCalculator.CalculateArea(GetBasePoints());
            float heightArea = AreaCalculator.CalculateArea(GetHeightPoints());
            Debug.Log("Dien tich = " + baseArea);
            Debug.Log("Dien tich = " + heightArea);

            // Hiển thị diện tích giữa các mặt
            Vector3 baseCenter = GetPolygonCenter(currentBasePoints);
            Vector3 topCenter = GetPolygonCenter(currentHeightPoints);

            if (flag == 1)
            {
                RoomModelBuilder roomBuilder1 = FindObjectOfType<RoomModelBuilder>();
                if (roomBuilder1 != null)
                {
                    List<Vector3> basePositions = GetBasePoints();
                    List<Vector3> heightPositions = GetHeightPoints();

                    roomBuilder1.SetRoomData(basePositions, heightPositions); // Truyền dữ liệu vào RoomModelBuilder
                    roomBuilder1.BuildWalls(); // Gọi vẽ vật liệu
                }
                // Tính và hiển thị diện tích của các mặt đứng (nếu cần)
                for (int i = 0; i < count; i++)
                {
                    Vector3 basePoint = currentBasePoints[i].transform.position;
                    Vector3 heightPoint = currentHeightPoints[i].transform.position;
                    Vector3 nextBasePoint = currentBasePoints[(i + 1) % count].transform.position;
                    Vector3 nextHeightPoint = currentHeightPoints[(i + 1) % count].transform.position;

                    // Tính diện tích cho mặt đứng giữa các điểm basePoint, heightPoint, nextBasePoint, nextHeightPoint
                    float sideArea = AreaCalculator.CalculateArea(new List<Vector3> { basePoint, heightPoint, nextBasePoint, nextHeightPoint });

                    // Hiển thị diện tích mặt đứng
                    Vector3 sideCenter = (basePoint + nextBasePoint) / 2 + (heightPoint + nextHeightPoint) / 2;
                    AreaCalculator.ShowAreaText(sideCenter, sideArea);
                }
            }

            List<GameObject> baseCopy = new List<GameObject>(currentBasePoints);
            List<GameObject> heightCopy = new List<GameObject>(currentHeightPoints);

            // Tính diện tích mặt đứng **phải làm ở đây**, trước khi clear
            for (int i = 0; i < count; i++)
            {
                Vector3 basePoint = baseCopy[i].transform.position;
                Vector3 heightPoint = heightCopy[i].transform.position;
                Vector3 nextBasePoint = baseCopy[(i + 1) % count].transform.position;
                Vector3 nextHeightPoint = heightCopy[(i + 1) % count].transform.position;

                float sideArea = AreaCalculator.CalculateArea(new List<Vector3> { basePoint, heightPoint, nextBasePoint, nextHeightPoint });
                Vector3 sideCenter = (basePoint + nextBasePoint) / 2 + (heightPoint + nextHeightPoint) / 2;
                AreaCalculator.ShowAreaText(sideCenter, sideArea);
            }

            // Lưu list vào tổng
            allBasePoints.Add(baseCopy);
            allHeightPoints.Add(heightCopy);

            // Clear cho mạch mới
            currentBasePoints.Clear();
            currentHeightPoints.Clear();
            referenceHeightPoint = null;

            lineManager.ShowAreaText(baseCenter, baseArea);
            lineManager.ShowAreaText(topCenter, heightArea);

            flag = 0;
            Debug.Log("[Unity] Mạch đã được lưu và sẵn sàng tạo mạch mới");
        }

        // Nối Pn với Pn' (điểm chiều cao)
        lineManager.DrawLineAndDistance(newBasePoint.transform.position, newHeightPoint.transform.position);

        RoomModelBuilder roomBuilder = FindObjectOfType<RoomModelBuilder>();
        if (roomBuilder != null)
        {
            List<Vector3> basePositions = GetBasePoints();
            List<Vector3> heightPositions = GetHeightPoints();

            roomBuilder.SetRoomData(basePositions, heightPositions); // Truyền dữ liệu vào RoomModelBuilder
            roomBuilder.BuildWalls(); // Gọi vẽ vật liệu
        }
    }

    GameObject GetOrCreatePoint(List<GameObject> points, Vector3 position)
    {
        foreach (GameObject point in points)
        {
            if (Vector3.Distance(point.transform.position, position) < mergeThreshold)
                return point; // Trả về điểm cũ nếu khoảng cách quá gần
        }
        GameObject newPoint = Instantiate(pointPrefab, position, Quaternion.identity);
        return newPoint;
    }

    public List<Vector3> GetBasePoints()
    {
        List<Vector3> basePositions = new List<Vector3>();
        foreach (GameObject point in currentBasePoints)
        {
            basePositions.Add(point.transform.position);
        }
        return basePositions;
    }

    public List<Vector3> GetHeightPoints()
    {
        List<Vector3> heightPositions = new List<Vector3>();
        foreach (GameObject point in currentHeightPoints)
        {
            heightPositions.Add(point.transform.position);
        }
        return heightPositions;
    }

    Vector3 GetPolygonCenter(List<GameObject> points)
    {
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point.transform.position;
        }
        return center / points.Count;
    }
    public List<List<GameObject>> GetAllBasePoints()
    {
        return allBasePoints;
    }

    public List<List<GameObject>> GetAllHeightPoints()
    {
        return allHeightPoints;
    }
}
