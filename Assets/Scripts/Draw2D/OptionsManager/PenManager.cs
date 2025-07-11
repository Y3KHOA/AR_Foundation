using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PenManager : MonoBehaviour
{
    public Button penButton;          // Button để bật/tắt chức năng pen
    public static Camera mainCamera;  // Camera chính để di chuyển và phóng to
    public float zoomSpeed = 2f;      // Tốc độ zoom
    public float panSpeed = 0.5f;     // Tốc độ di chuyển

    public static bool isPenActive = true; // Trạng thái của Pen (bật/tắt)
    private CheckpointManager checkpointManager; // Tham chiếu đến CheckpointManager để điều khiển vẽ

    // public bool IsPenActive => isPenActive;  // Getter để cung cấp trạng thái Pen

    void Start()
    {
        mainCamera = Camera.main;
        // Gán sự kiện click vào Button
        penButton.onClick.AddListener(TogglePen);

        // Lấy tham chiếu đến CheckpointManager
        checkpointManager = FindObjectOfType<CheckpointManager>();

        // Đảm bảo trạng thái ban đầu của Pen là tắt
        UpdatePenState();
    }

    void Update()
    {
        if (isPenActive)
        {
            // Nếu Pen đang bật, cho phép vẽ
            checkpointManager.enabled = true;  // Bật chế độ vẽ trong CheckpointManager
            HandleZoomAndPan(false);  // Tắt zoom và di chuyển khi vẽ
        }
        else
        {
            // Nếu Pen đang tắt, cho phép di chuyển và zoom camera
            checkpointManager.enabled = false; // Tắt chế độ vẽ trong CheckpointManager
            HandleZoomAndPan(true);  // Bật zoom và di chuyển khi không vẽ
        }
    }

    // Hàm xử lý zoom và di chuyển camera
    public void HandleZoomAndPan(bool canZoomAndPan)
    {
        if (!canZoomAndPan) return;

        // Zoom bằng cuộn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // Điều chỉnh kích thước của camera, giới hạn phạm vi zoom
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, 1f, 20f);
        }

        // Di chuyển camera bằng chuột (hoặc bằng 1 ngón tay)
        if (Input.touchCount == 1) // Nếu chỉ có 1 ngón tay trên màn hình
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // Tính toán sự thay đổi của vị trí màn hình để di chuyển camera
                Vector3 touchDelta = touch.deltaPosition;  // Sự thay đổi của vị trí touch
                Vector3 move = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, mainCamera.nearClipPlane)) -
               mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x - touchDelta.x, touch.position.y - touchDelta.y, mainCamera.nearClipPlane));

                mainCamera.transform.Translate(-move, Space.World);
            }
        }

        // Phóng to/thu nhỏ bằng hai ngón tay (pinch-to-zoom)
        if (Input.touchCount == 2) // Nếu có 2 ngón tay trên màn hình
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Tính toán sự thay đổi khoảng cách giữa 2 ngón tay
            float prevMagnitude = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;
            Debug.Log($"prevMagnitude: {prevMagnitude}, currentMagnitude: {currentMagnitude}");

            // Tính toán sự thay đổi khoảng cách và điều chỉnh kích thước camera
            float difference = currentMagnitude - prevMagnitude;

            // Điều chỉnh kích thước camera dựa trên sự thay đổi khoảng cách giữa 2 ngón tay
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - difference * zoomSpeed * 0.01f, 1f, 20f);
        }
    }

    // Toggle trạng thái Pen
    void TogglePen()
    {
        isPenActive = !isPenActive;  // Chuyển trạng thái Pen
        UpdatePenState();            // Cập nhật trạng thái Pen
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
}
