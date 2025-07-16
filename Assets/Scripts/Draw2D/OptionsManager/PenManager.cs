using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PenManager : MonoBehaviour
{
    public Button penButton;          // Button để bật/tắt chức năng pen
    public static Camera mainCamera;  // Camera chính để di chuyển và phóng to
    public float zoomSpeed = 2f;      // Tốc độ zoom
    public float panSpeed = 0.5f;     // Tốc độ di chuyển
    public static bool isRoomFloorBeingDragged = false;

    public static bool isPenActive = true; // Trạng thái của Pen (bật/tắt)
    private CheckpointManager checkpointManager; // Tham chiếu đến CheckpointManager để điều khiển vẽ

    private ToggleColor toggleColor;
    // public bool IsPenActive => isPenActive;  // Getter để cung cấp trạng thái Pen

    void Start()
    {
        mainCamera = Camera.main;
        // Gán sự kiện click vào Button
        penButton.onClick.AddListener(TogglePen);
        toggleColor = penButton.GetComponent<ToggleColor>();
        toggleColor.Toggle(isPenActive);
        // Lấy tham chiếu đến CheckpointManager
        checkpointManager = FindObjectOfType<CheckpointManager>();

        // Đảm bảo trạng thái ban đầu của Pen là tắt
        UpdatePenState();
    }

    void Update()
    {
        if (isPenActive)
        {
            checkpointManager.enabled = true;
            HandleZoomAndPan(false); // Tắt zoom khi vẽ
        }
        else
        {
            checkpointManager.enabled = false;

            // Nếu mesh sàn đang drag ➜ khóa bàn cờ
            if (isRoomFloorBeingDragged)
            {
                Debug.Log("RoomFloor drag đang hoạt động ➜ Khóa pan/zoom");
                HandleZoomAndPan(false);
            }
            else
            {
                HandleZoomAndPan(true);
            }
        }
    }

    // Hàm xử lý zoom và di chuyển camera
    public void HandleZoomAndPan(bool canZoomAndPan)
    {
        if (!canZoomAndPan) return;
        if (IsTouchOverRoomFloor())
        {
            Debug.Log("Đang chạm RoomFloor ➜ KHÔNG zoom/pan!");
            return;
        }

        // ✨ Thêm kiểm tra raycast vào đầu tiên
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.CompareTag("RoomFloor"))
                    {
                        Debug.Log("🛑 Raycast đang hit RoomFloor ➜ Bỏ pan/zoom bàn cờ!");
                        return; // Chặn bàn cờ ngay từ đầu
                    }
                }
            }
        }

        // Zoom bằng cuộn chuột (Editor/PC)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, 1f, 70f);
        }

        // Di chuyển camera bằng chuột hoặc bằng 1 ngón tay
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 touchDelta = touch.deltaPosition;
                Vector3 move = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, mainCamera.nearClipPlane)) -
                               mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x - touchDelta.x, touch.position.y - touchDelta.y, mainCamera.nearClipPlane));

                mainCamera.transform.Translate(-move, Space.World);
            }
        }

        // Phóng to/thu nhỏ bằng hai ngón tay (pinch-to-zoom)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float prevMagnitude = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - difference * zoomSpeed * 0.01f, 1f, 70f);
        }
    }

    // Toggle trạng thái Pen
    void TogglePen()
    {
        isPenActive = !isPenActive;  // Chuyển trạng thái Pen
        UpdatePenState();            // Cập nhật trạng thái Pen
        toggleColor.Toggle(isPenActive);

    }

    // Cập nhật trạng thái Pen
    void UpdatePenState()
    {
        try
        {
            penButton.GetComponentInChildren<Text>().text = isPenActive ? "Turn Off Pen" : "Turn On Pen";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Lỗi khi cập nhật button text: " + e.Message);
        }
    }
    private bool IsTouchOverRoomFloor()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomFloor");
        }
#else
    if (Input.touchCount > 0)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomFloor");
    }
#endif
        return false;
    }

}
