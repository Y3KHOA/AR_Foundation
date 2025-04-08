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
    private List<GameObject> basePoints = new List<GameObject>();
    private List<GameObject> heightPoints = new List<GameObject>();
    private GameObject referenceHeightPoint = null;
    private List<MeasurementData> allMeasurements = new List<MeasurementData>();

    private bool isPointVisible = false;
    private bool hasPlane = false;
    private float mergeThreshold = 0.1f; // Ngưỡng hợp nhất điểm
    private float closeThreshold = 0.2f; // Ngưỡng khép kín đường
    private int flag = 0; // Mặc định flag = 0
    public int Flag { get { return flag; } }

    void Start()
    {
        // Lấy đơn vị đo từ bộ nhớ
        string unit = PlayerPrefs.GetString("SelectedUnit", "m");
        float savedHeight = PlayerPrefs.GetFloat("HeightValue", 0f);
        this.heightValue = savedHeight;

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
            if (basePoints.Count > 0)
            {
                Vector3 lastBasePoint = basePoints[basePoints.Count - 1].transform.position;
                Vector3 previewPos = previewPoint.transform.position;

                Debug.Log($"[Update] Draw PreviewLine from {lastBasePoint} to {previewPos}");
                lineManager.DrawPreviewLine(lastBasePoint, previewPos);
            }
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

    void HandleButtonClick()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Debug.Log("Không tìm thấy mặt phẳng để đặt object.");
            return;
        }

        Pose hitPose = hits[0].pose;
        GameObject newBasePoint = GetOrCreatePoint(basePoints, hitPose.position);
        GameObject newHeightPoint = referenceHeightPoint != null
            ? GetOrCreatePoint(heightPoints, new Vector3(hitPose.position.x, referenceHeightPoint.transform.position.y, hitPose.position.z))
            : GetOrCreatePoint(heightPoints, hitPose.position + new Vector3(0, heightValue, 0));

        if (referenceHeightPoint == null)
            referenceHeightPoint = newHeightPoint;

        basePoints.Add(newBasePoint);
        heightPoints.Add(newHeightPoint);

        int count = basePoints.Count;

        // Tự động nối Pn với Pn-1
        if (count > 1)
        {
            lineManager.DrawLineAndDistance(basePoints[count - 2].transform.position, newBasePoint.transform.position);
            lineManager.DrawLineAndDistance(heightPoints[count - 2].transform.position, newHeightPoint.transform.position);
        }

        // Kiểm tra nếu Pn gần P1, tự động khép kín đường
        if (count > 2 && Vector3.Distance(newBasePoint.transform.position, basePoints[0].transform.position) < closeThreshold)
        {
            lineManager.DrawLineAndDistance(newBasePoint.transform.position, basePoints[0].transform.position);
            lineManager.DrawLineAndDistance(newHeightPoint.transform.position, heightPoints[0].transform.position);
            flag = 1; // Đánh dấu đã khép kín đường
            Debug.Log("[unity1] flag=1");

            // Tính diện tích giữa các mặt đáy và mặt trên
            float baseArea = AreaCalculator.CalculateArea(GetBasePoints());
            float heightArea = AreaCalculator.CalculateArea(GetHeightPoints());
            Debug.Log("Dien tich = " + baseArea);
            Debug.Log("Dien tich = " + heightArea);
            Debug.Log("[unity2] flag=1");

            // Hiển thị diện tích giữa các mặt
            Vector3 baseCenter = GetPolygonCenter(basePoints);
            Vector3 topCenter = GetPolygonCenter(heightPoints);
            Debug.Log("[unity3] flag=1");

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
            }
            Debug.Log("[unity4] flag=1");

            // Tính và hiển thị diện tích của các mặt đứng (nếu cần)
            for (int i = 0; i < count; i++)
            {
                Vector3 basePoint = basePoints[i].transform.position;
                Vector3 heightPoint = heightPoints[i].transform.position;
                Vector3 nextBasePoint = basePoints[(i + 1) % count].transform.position;
                Vector3 nextHeightPoint = heightPoints[(i + 1) % count].transform.position;

                // Tính diện tích cho mặt đứng giữa các điểm basePoint, heightPoint, nextBasePoint, nextHeightPoint
                float sideArea = AreaCalculator.CalculateArea(new List<Vector3> { basePoint, heightPoint, nextBasePoint, nextHeightPoint });

                // Hiển thị diện tích mặt đứng
                Vector3 sideCenter = (basePoint + nextBasePoint) / 2 + (heightPoint + nextHeightPoint) / 2;
                AreaCalculator.ShowAreaText(sideCenter, sideArea);
            }
            lineManager.ShowAreaText(baseCenter, baseArea);
            lineManager.ShowAreaText(topCenter, heightArea);
            Debug.Log("[unity5] flag=1");
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
        foreach (GameObject point in basePoints)
        {
            basePositions.Add(point.transform.position);
        }
        return basePositions;
    }

    /*************  ✨ Windsurf Command ⭐  *************/
    /// <summary>
    /// Retrieves a list of positions for the height points.
    /// </summary>
    /// <returns>A list of Vector3 positions representing the height points.</returns>

    /*******  42db1563-e22a-48c9-954e-d1cb3166e95b  *******/
    public List<Vector3> GetHeightPoints()
    {
        List<Vector3> heightPositions = new List<Vector3>();
        foreach (GameObject point in heightPoints)
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
}
