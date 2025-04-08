using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
public class TestAxisY : MonoBehaviour
{
    [Header("UI Elements")]
    public Button toggleButton;

    [Header("AR Elements")]
    public GameObject pointPrefab;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public LineManager lineManager;

    private GameObject previewPoint = null;  // 🔹 Điểm xem trước
    private GameObject spawnedPoint;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> basePoints = new List<GameObject>();
    private List<GameObject> heightPoints = new List<GameObject>();
    private GameObject referenceHeightPoint = null;
    private bool isSettingBasePoint = true;

    private bool isPointVisible = false;
    private bool hasPlane = false;
    private float mergeThreshold = 0.1f; // Ngưỡng hợp nhất điểm
    private float closeThreshold = 0.2f; // Ngưỡng khép kín đường

    void Start()
    {
        if (toggleButton == null)
            Debug.LogError("toggleButton chưa được thiết lập!");

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

            // 🔹 Đảm bảo previewPoint tồn tại
            if (previewPoint == null)
            {
                previewPoint = Instantiate(pointPrefab, hitPose.position, Quaternion.identity);
                previewPoint.name = "PreviewPoint";
            }

            // Cập nhật vị trí preview point
            previewPoint.transform.position = hitPose.position;

            // 🔹 Chỉ vẽ preview line nếu có ít nhất 1 điểm cơ sở
            if (basePoints.Count > 0)
            {
                Vector3 lastBasePoint = basePoints[basePoints.Count - 1].transform.position;
                Vector3 previewPos = previewPoint.transform.position;

                Debug.Log($"🟡 [Update] Vẽ PreviewLine từ {lastBasePoint} đến {previewPos}");
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
}
