using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;

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
    public ModelView modelView;
    public float heightValue = 0.5f;
    public float AreaValue = 0f; // Diện tích mặt đáy
    public float PerimeterValue = 0f; // Chu vi (tổng chiều dài các cạnh)
    public float CeilingValue = 0f; // Diện tích mặt trần

    private GameObject previewPoint = null;  // Điểm xem trước
    private GameObject spawnedPoint;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<List<GameObject>> allBasePoints = new List<List<GameObject>>();
    private List<List<GameObject>> allHeightPoints = new List<List<GameObject>>();

    public List<List<GameObject>> AllBasePoints { get { return allBasePoints; } }
    public List<List<GameObject>> AllHeightPoints { get { return allHeightPoints; } }

    public LineType currentLineType = LineType.Wall;
    public List<WallLine> segmentWallLines = new List<WallLine>();
    public List<Room> rooms = new List<Room>();
    // private Room room = new Room();

    private List<GameObject> currentBasePoints = new List<GameObject>();
    private List<GameObject> currentHeightPoints = new List<GameObject>();
    private GameObject referenceHeightPoint = null;

    private bool isPointVisible = false;
    private bool hasPlane = false;
    private float mergeThreshold = 0.1f; // Ngưỡng hợp nhất điểm
    private float closeThreshold = 0.2f; // Ngưỡng khép kín đường
    private int flag = 0; // Mặc định flag = 0
    public int Flag { get { return flag; } set { flag = value; } }
    private bool measure = true;
    private Vector3 fixedBasePointPosition;
    private float initialCameraPitch;
    private GameObject tempBasePoint;
    private bool isDoor = false;
    private bool isWindow = false;
    private float heightDoor = 0.5f;
    private GameObject firstDoorBasePoint = null;
    private GameObject firstDoorTopPoint = null;
    private bool measureRoom =false;
    private bool isMeasuringDoorHeight = false;
    private bool isMeasuringWindowHeight = false;


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
        isDoor = PanelManagerDoorWindow.Instance.IsDoorChanged;
        isWindow = PanelManagerDoorWindow.Instance.IsWindowChanged;

        if (!hasPlane || !isPointVisible || pointPrefab == null || raycastManager == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (measure && !isDoor && !isWindow)
        {

            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (spawnedPoint == null)
                    spawnedPoint = Instantiate(pointPrefab, hitPose.position, Quaternion.identity);
                else
                    spawnedPoint.transform.position = hitPose.position;

                // Đảm bảo previewPoint tồn tại
                if (previewPoint == null)
                {
                    previewPoint = Instantiate(pointPrefab, hitPose.position, Quaternion.identity);
                    previewPoint.name = "PreviewPoint";
                }

                // Cập nhật vị trí preview point ban đầu
                previewPoint.transform.position = hitPose.position;

                // === BÂY GIỜ MỚI TIẾN HÀNH VẼ ===
                if (currentBasePoints.Count > 0)
                {
                    Vector3 lastBasePoint = currentBasePoints[currentBasePoints.Count - 1].transform.position;
                    Vector3 previewPos = previewPoint.transform.position;

                    lineManager.DrawPreviewLine(lastBasePoint, previewPos);

                    Vector3 base1 = lastBasePoint;
                    Vector3 top1 = currentHeightPoints.Count > 0 ? currentHeightPoints[currentHeightPoints.Count - 1].transform.position : base1 + Vector3.up * 0.5f;
                    Vector3 base2 = previewPos;
                    Vector3 top2 = previewPos + new Vector3(0, heightValue, 0);

                    modelView.DrawPreviewWall(base1, top1, base2, top2);
                }
            }
        }
        else
        // === khi nhan door ===
        if (isDoor)
        {
            if (!isMeasuringDoorHeight)
            {
                Camera cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
                if (cam == null) return;

                Vector3 centerWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f)); // điểm giữa màn hình

                WallLine matchedLine = null;
                float minDistance = float.MaxValue;

                foreach (Room room in RoomStorage.rooms)
                {
                    foreach (WallLine line in room.wallLines)
                    {
                        Vector3 projected = ProjectPointOnLineSegment(line.start, line.end, centerWorld);
                        float distance = Vector3.Distance(centerWorld, projected);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            matchedLine = line;
                        }
                    }
                }

                if (matchedLine != null)
                {
                    // Tạo checkpoint nếu chưa có
                    if (previewPoint == null)
                    {
                        previewPoint = Instantiate(pointPrefab, matchedLine.start + Vector3.up * 0.01f, Quaternion.identity);
                        previewPoint.name = "DoorPreview";
                    }

                    // Di chuyển previewPoint trượt ngang trên đoạn tường
                    Vector3 projected = ProjectPointOnLineSegment(matchedLine.start, matchedLine.end, centerWorld);
                    float y = previewPoint.transform.position.y; // giữ nguyên Y ban đầu
                    previewPoint.transform.position = new Vector3(projected.x, y, projected.z);

                    previewPoint.SetActive(true);
                }
                else
                {
                    if (previewPoint != null)
                        previewPoint.SetActive(false);
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
        else
        // === khi nhan window ===
        if (isWindow)
        {
            if (!isMeasuringWindowHeight)
            {
                Camera cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
                if (cam == null) return;

                Vector3 centerWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f)); // tâm màn hình

                Vector3? bestPoint = null;
                float minDistance = float.MaxValue;

                foreach (Room room in RoomStorage.rooms)
                {
                    for (int i = 0; i < room.wallLines.Count; i++)
                    {
                        WallLine line = room.wallLines[i];
                        float height = room.heights[i];

                        Vector3 baseStart = line.start;
                        Vector3 baseEnd = line.end;
                        Vector3 topStart = baseStart + Vector3.up * height;

                        // Tạo mặt phẳng của tường từ 3 điểm
                        Plane wallPlane = new Plane(baseStart, baseEnd, topStart);

                        Ray ray = new Ray(cam.transform.position, centerWorld - cam.transform.position);
                        if (wallPlane.Raycast(ray, out float enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);

                            // Kiểm tra nếu hitPoint nằm trong đoạn tường theo chiều ngang và cao
                            Vector3 projected = ProjectPointOnLineSegment(baseStart, baseEnd, hitPoint);
                            float y = hitPoint.y;
                            float baseY = baseStart.y;

                            if (y >= baseY && y <= baseY + height)
                            {
                                float distance = Vector3.Distance(centerWorld, hitPoint);
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    bestPoint = hitPoint;
                                }
                            }
                        }
                    }
                }

                if (bestPoint.HasValue)
                {
                    if (previewPoint == null)
                    {
                        previewPoint = Instantiate(pointPrefab, bestPoint.Value, Quaternion.identity);
                        previewPoint.name = "WindowPreview";
                    }

                    previewPoint.transform.position = bestPoint.Value;
                    previewPoint.SetActive(true);
                }
                else
                {
                    if (previewPoint != null)
                        previewPoint.SetActive(false);
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
        
        // === khi nhan height=00 ===
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

    private bool IsSameSegment2D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, float tolerance = 0.01f)
    {
        Vector2 A1 = new Vector2(a1.x, a1.z);
        Vector2 A2 = new Vector2(a2.x, a2.z);
        Vector2 B1 = new Vector2(b1.x, b1.z);
        Vector2 B2 = new Vector2(b2.x, b2.z);

        return (Vector2.Distance(A1, B1) < tolerance && Vector2.Distance(A2, B2) < tolerance)
            || (Vector2.Distance(A1, B2) < tolerance && Vector2.Distance(A2, B1) < tolerance);
    }

    /// <summary>
    /// Hàm xử lý tổng khi nhấn create button 
    /// </summary>
    void HandleButtonClick()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (isDoor)
        {
            Debug.Log("Insert checkpoint to line (Door)");

            if (previewPoint == null)
            {
                Debug.LogError("PreviewPoint is null!");
                return;
            }

            Vector3 currentPos = previewPoint.transform.position;
            float minDistance = float.MaxValue;
            WallLine targetWall = null;
            Room targetRoom = null;

            foreach (Room room in RoomStorage.rooms)
            {
                foreach (WallLine wall in room.wallLines)
                {
                    Vector3 projected = ProjectPointOnLineSegment(wall.start, wall.end, currentPos);
                    float dist = Vector3.Distance(currentPos, projected);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        targetWall = wall;
                        targetRoom = room;
                    }
                }
            }

            if (targetWall != null && targetRoom != null)
            {
                Vector3 insertPoint = ProjectPointOnLineSegment(targetWall.start, targetWall.end, currentPos);

                if (PanelManagerDoorWindow.Instance.MeasureMode)
                {
                    if (!isMeasuringDoorHeight)
                    {
                        // Lần nhấn đầu tiên: bắt đầu đo
                        isMeasuringDoorHeight = true;

                        tempBasePoint = GetOrCreatePoint(currentBasePoints, insertPoint);
                        fixedBasePointPosition = tempBasePoint.transform.position;

                        Camera cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
                        if (cam != null)
                            initialCameraPitch = cam.transform.eulerAngles.x;

                        return;
                    }
                    else
                    {
                        // Lần nhấn thứ 2: kết thúc đo
                        Debug.Log("lan nhan 2: do heightDoor");

                        if (previewPoint != null)
                        {
                            heightDoor = previewPoint.transform.position.y - fixedBasePointPosition.y;
                            heightDoor = Mathf.Max(0, heightDoor);
                            Debug.Log("heightDoor = " + heightDoor);

                            lineManager.DestroyPreviewObjects();
                            previewPoint.SetActive(false);
                            Destroy(previewPoint); previewPoint = null;

                            if (tempBasePoint != null)
                            {
                                if (currentBasePoints.Contains(tempBasePoint))
                                    currentBasePoints.Remove(tempBasePoint);
                                Destroy(tempBasePoint);
                                tempBasePoint = null;
                            }

                            isMeasuringDoorHeight = false;
                            fixedBasePointPosition = Vector3.zero;
                            initialCameraPitch = 0f;
                            PanelManagerDoorWindow.Instance.MeasureMode= false;

                            return;
                        }
                        else
                        {
                            Debug.LogWarning("previewPoint null khi đo chiều cao.");
                        }
                    }
                }
                
                // Lần nhấn đầu tiên tạo điểm đầu cửa
                    if (firstDoorBasePoint == null)
                    {
                        firstDoorBasePoint = Instantiate(pointPrefab, insertPoint, Quaternion.identity);
                        firstDoorTopPoint = Instantiate(pointPrefab, insertPoint + Vector3.up * heightDoor, Quaternion.identity);

                        // Kết nối điểm Pn với Pn'
                        lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, firstDoorTopPoint.transform.position);
                        // return;
                    }
                    else
                    {
                        // Lần nhấn thứ hai tạo điểm cuối cửa
                        GameObject secondDoorBasePoint = Instantiate(pointPrefab, insertPoint, Quaternion.identity);
                        GameObject secondDoorTopPoint = Instantiate(pointPrefab, insertPoint + Vector3.up * heightDoor, Quaternion.identity);

                        // 1. Kết nối Pn với Pn' của mỗi điểm
                        lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, firstDoorTopPoint.transform.position);   // p1 ↔ p1'
                        lineManager.DrawLineAndDistance(secondDoorBasePoint.transform.position, secondDoorTopPoint.transform.position); // p2 ↔ p2'

                        // 2. Kết nối base và top: p1 → p2, p1' → p2'
                        lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, secondDoorBasePoint.transform.position); // p1 → p2
                        lineManager.DrawLineAndDistance(firstDoorTopPoint.transform.position, secondDoorTopPoint.transform.position);   // p1' → p2'

                        // Vẽ tường cửa riêng biệt
                        modelView.CreateWall(
                            firstDoorBasePoint.transform.position,
                            secondDoorBasePoint.transform.position,
                            firstDoorTopPoint.transform.position,
                            secondDoorTopPoint.transform.position
                        );

                        // Lưu vào Room (đoạn cửa)
                        Vector2 doorStart = new Vector2(firstDoorBasePoint.transform.position.x, firstDoorBasePoint.transform.position.z);
                        Vector2 doorEnd = new Vector2(secondDoorBasePoint.transform.position.x, secondDoorBasePoint.transform.position.z);

                        // === Chèn checkpoint cửa vào giữa đúng vị trí ===
                        List<Vector2> pts = targetRoom.checkpoints;
                        List<float> hts = targetRoom.heights;

                        int insertIndex = -1;
                        for (int i = 0; i < pts.Count - 1; i++)
                        {
                            Vector3 a = new Vector3(pts[i].x, 0, pts[i].y);
                            Vector3 b = new Vector3(pts[i + 1].x, 0, pts[i + 1].y);
                            if (IsSameSegment2D(a, b, targetWall.start, targetWall.end))
                            {
                                insertIndex = i;
                                break;
                            }
                        }

                        if (insertIndex != -1)
                        {
                            pts.Insert(insertIndex + 1, doorStart);
                            hts.Insert(insertIndex + 1, heightDoor);
                            pts.Insert(insertIndex + 2, doorEnd);
                            hts.Insert(insertIndex + 2, heightDoor);
                        }
                        else
                        {
                            Debug.LogWarning("Không tìm thấy đoạn để chèn cửa. Thêm vào cuối.");
                            pts.Add(doorStart); hts.Add(heightDoor);
                            pts.Add(doorEnd); hts.Add(heightDoor);
                        }

                        // === Cập nhật lại wallLines: chia đoạn ban đầu thành 3 ===
                        targetRoom.wallLines.Remove(targetWall);

                        Vector3 leftStart = targetWall.start;
                        Vector3 leftEnd = firstDoorBasePoint.transform.position;
                        Vector3 rightStart = secondDoorBasePoint.transform.position;
                        Vector3 rightEnd = targetWall.end;

                        targetRoom.wallLines.Add(new WallLine(leftStart, leftEnd, LineType.Wall, 0f, heightValue)); // hoặc chiều cao tường gốc
                        targetRoom.wallLines.Add(new WallLine(firstDoorBasePoint.transform.position, secondDoorBasePoint.transform.position, LineType.Door, 0f, heightDoor));
                        targetRoom.wallLines.Add(new WallLine(rightStart, rightEnd, LineType.Wall, 0f, heightValue)); // hoặc chiều cao tường gốc

                        Debug.Log("Door completed.");

                        // Reset trạng thái về ban đầu
                        firstDoorBasePoint = null;
                        firstDoorTopPoint = null;

                        isDoor = false;
                        PanelManagerDoorWindow.Instance.IsDoorChanged = false;
                        PanelManagerDoorWindow.Instance.IsClicked = true;
                    }
            }

            return; // Thoát sau khi xử lý cửa
        }

        if (isWindow)
        {
            Debug.Log("Insert checkpoint to line (Door)");

            if (previewPoint == null)
            {
                Debug.LogError("PreviewPoint is null!");
                return;
            }

            Vector3 currentPos = previewPoint.transform.position;
            float minDistance = float.MaxValue;
            WallLine targetWall = null;
            Room targetRoom = null;

            foreach (Room room in RoomStorage.rooms)
            {
                foreach (WallLine wall in room.wallLines)
                {
                    Vector3 projected = ProjectPointOnLineSegment(wall.start, wall.end, currentPos);
                    float dist = Vector3.Distance(currentPos, projected);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        targetWall = wall;
                        targetRoom = room;
                    }
                }
            }

            if (targetWall != null && targetRoom != null)
            {
                // Vector3 insertPoint = ProjectPointOnLineSegment(targetWall.start, targetWall.end, currentPos);
                Vector3 insertPoint = currentPos;

                if (PanelManagerDoorWindow.Instance.MeasureMode)
                {
                    if (!isMeasuringWindowHeight)
                    {
                        // Lần nhấn đầu tiên: bắt đầu đo
                        isMeasuringWindowHeight = true;

                        tempBasePoint = GetOrCreatePoint(currentBasePoints, insertPoint);
                        fixedBasePointPosition = tempBasePoint.transform.position;

                        Camera cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
                        if (cam != null)
                            initialCameraPitch = cam.transform.eulerAngles.x;

                        return;
                    }
                    else
                    {
                        // Lần nhấn thứ 2: kết thúc đo
                        Debug.Log("lan nhan 2: do heightDoor");

                        if (previewPoint != null)
                        {
                            heightDoor = previewPoint.transform.position.y - fixedBasePointPosition.y;
                            heightDoor = Mathf.Max(0, heightDoor);
                            Debug.Log("heightDoor = " + heightDoor);

                            lineManager.DestroyPreviewObjects();
                            previewPoint.SetActive(false);
                            Destroy(previewPoint); previewPoint = null;

                            if (tempBasePoint != null)
                            {
                                if (currentBasePoints.Contains(tempBasePoint))
                                    currentBasePoints.Remove(tempBasePoint);
                                Destroy(tempBasePoint);
                                tempBasePoint = null;
                            }

                            isMeasuringWindowHeight = false;
                            fixedBasePointPosition = Vector3.zero;
                            initialCameraPitch = 0f;
                            PanelManagerDoorWindow.Instance.MeasureMode = false;

                            return;
                        }
                        else
                        {
                            Debug.LogWarning("previewPoint null khi đo chiều cao.");
                        }
                    }
                }

                // Lần nhấn đầu tiên tạo điểm đầu cửa
                if (firstDoorBasePoint == null)
                {
                    firstDoorBasePoint = Instantiate(pointPrefab, insertPoint, Quaternion.identity);
                    firstDoorTopPoint = Instantiate(pointPrefab, insertPoint + Vector3.up * heightDoor, Quaternion.identity);

                    // Kết nối điểm Pn với Pn'
                    lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, firstDoorTopPoint.transform.position);
                    // return;
                }
                else
                {
                    // Lần nhấn thứ hai tạo điểm cuối cửa
                    GameObject secondDoorBasePoint = Instantiate(pointPrefab, insertPoint, Quaternion.identity);
                    GameObject secondDoorTopPoint = Instantiate(pointPrefab, insertPoint + Vector3.up * heightDoor, Quaternion.identity);

                    Vector3 p1 = firstDoorBasePoint.transform.position;
                    Vector3 p2 = secondDoorBasePoint.transform.position;

                    // 1. Kết nối Pn với Pn' của mỗi điểm
                    lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, firstDoorTopPoint.transform.position);   // p1 ↔ p1'
                    lineManager.DrawLineAndDistance(secondDoorBasePoint.transform.position, secondDoorTopPoint.transform.position); // p2 ↔ p2'

                    // 2. Kết nối base và top: p1 → p2, p1' → p2'
                    lineManager.DrawLineAndDistance(firstDoorBasePoint.transform.position, secondDoorBasePoint.transform.position); // p1 → p2
                    lineManager.DrawLineAndDistance(firstDoorTopPoint.transform.position, secondDoorTopPoint.transform.position);   // p1' → p2'

                    // Vẽ tường cửa riêng biệt
                    modelView.CreateWall(
                        firstDoorBasePoint.transform.position,
                        secondDoorBasePoint.transform.position,
                        firstDoorTopPoint.transform.position,
                        secondDoorTopPoint.transform.position
                    );

                    // Lưu vào Room (đoạn cửa)
                    Vector2 doorStart = new Vector2(firstDoorBasePoint.transform.position.x, firstDoorBasePoint.transform.position.z);
                    Vector2 doorEnd = new Vector2(secondDoorBasePoint.transform.position.x, secondDoorBasePoint.transform.position.z);

                    // === Chèn checkpoint cửa vào giữa đúng vị trí ===
                    List<Vector2> pts = targetRoom.checkpoints;
                    List<float> hts = targetRoom.heights;

                    int insertIndex = -1;
                    for (int i = 0; i < pts.Count - 1; i++)
                    {
                        Vector3 a = new Vector3(pts[i].x, 0, pts[i].y);
                        Vector3 b = new Vector3(pts[i + 1].x, 0, pts[i + 1].y);
                        if (IsSameSegment2D(a, b, targetWall.start, targetWall.end))
                        {
                            insertIndex = i;
                            break;
                        }
                    }

                    if (insertIndex != -1)
                    {
                        pts.Insert(insertIndex + 1, doorStart);
                        hts.Insert(insertIndex + 1, heightDoor);
                        pts.Insert(insertIndex + 2, doorEnd);
                        hts.Insert(insertIndex + 2, heightDoor);
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy đoạn để chèn cửa. Thêm vào cuối.");
                        pts.Add(doorStart); hts.Add(heightDoor);
                        pts.Add(doorEnd); hts.Add(heightDoor);
                    }

                    // === Cập nhật lại wallLines: chia đoạn ban đầu thành 3 ===
                    targetRoom.wallLines.Remove(targetWall);

                    Vector3 leftStart = targetWall.start;
                    Vector3 leftEnd = firstDoorBasePoint.transform.position;
                    Vector3 rightStart = secondDoorBasePoint.transform.position;
                    Vector3 rightEnd = targetWall.end;

                    targetRoom.wallLines.Add(new WallLine(leftStart, leftEnd, LineType.Wall, 0f, heightValue));
                    float distanceHeight = Mathf.Min(firstDoorBasePoint.transform.position.y, secondDoorBasePoint.transform.position.y);
                    float heightWindow = Mathf.Abs(firstDoorTopPoint.transform.position.y - firstDoorBasePoint.transform.position.y);

                    targetRoom.wallLines.Add(new WallLine(
                        firstDoorBasePoint.transform.position,
                        secondDoorBasePoint.transform.position,
                        LineType.Window,
                        distanceHeight,
                        heightWindow
                    ));
                    targetRoom.wallLines.Add(new WallLine(rightStart, rightEnd, LineType.Wall, 0f, heightValue));

                    Debug.Log("Window completed.");

                    // Reset trạng thái về ban đầu
                    firstDoorBasePoint = null;
                    firstDoorTopPoint = null;

                    isWindow = false;
                    PanelManagerDoorWindow.Instance.IsWindowChanged = false;
                    PanelManagerDoorWindow.Instance.IsClicked = true;
                }
            }

            return; // Thoát sau khi xử lý cửa sổ
        }

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

            modelView.CreateWall(currentBasePoints[count - 2].transform.position, newBasePoint.transform.position, currentHeightPoints[count - 2].transform.position, newHeightPoint.transform.position);
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
            Debug.Log("Dien tich base = " + baseArea); // Diện tích đáy
            Debug.Log("Dien tich height = " + heightArea); // Diện tích mặt trên
            AreaValue = baseArea;
            CeilingValue = heightArea;
            // PerimeterValue = AreaCalculator.CalculateArea(currentBasePoints); // Tính chu vi (tổng chiều dài các cạnh)

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

            Debug.Log("Done 0: " + baseCopy.Count);
            for (int i = 0; i < baseCopy.Count; i++)
            {
                Vector3 start = baseCopy[i].transform.position;
                Vector3 end = baseCopy[(i + 1) % baseCopy.Count].transform.position;

                WallLine wl = new WallLine(start, end, currentLineType, 0f, heightValue);
                // wallLines.Add(wl);             // Thêm vào tổng
                segmentWallLines.Add(wl);
            }
            // Lưu chính xác các WallLine này vào Room hiện tại
            Room room = new Room();
            room.wallLines.AddRange(segmentWallLines);
            Debug.Log("Done 1: " + segmentWallLines.Count);

            List<Vector2> path2D = new List<Vector2>();
            List<float> heightList = new List<float>();

            for (int j = 0; j < baseCopy.Count; j++)
            {
                Vector3 basePos = baseCopy[j].transform.position;
                Vector3 heightPos = heightCopy[j].transform.position;

                path2D.Add(new Vector2(basePos.x, basePos.z));
                heightList.Add(heightPos.y - basePos.y);
            }
            room.checkpoints.AddRange(path2D);
            room.heights.AddRange(heightList);
            Debug.Log("Done 3");

            RoomStorage.rooms.Add(room);

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
            Debug.Log("Done Room");
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

    Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t); // Giới hạn trong đoạn [0,1]
        return a + ab * t;
    }

}