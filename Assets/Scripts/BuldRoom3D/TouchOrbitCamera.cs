using UnityEngine;

public class TouchOrbitCamera : MonoBehaviour
{
    public Transform target;               // Đối tượng trung tâm (model)
    public float rotationSpeed = 0.2f;     // Tốc độ xoay
    public float zoomSpeed = 0.01f;
    public float panSpeed = 0.005f;

    public float minYAngle = -30f;
    public float maxYAngle = 80f;
    public float minDistance = 1f;
    public float maxDistance = 10f;

    private float currentX = 0f;
    private float currentY = 20f;
    private float distance = 5f;

    private Vector2 lastTouchPos;
    private bool isDragging = false;
    private bool isPanningMode = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    private Camera cam;
    private Vector3 pivotPoint;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("PreviewCamera").GetComponent<Camera>();

        if (target != null)
        {
            pivotPoint = target.position;
        }
        else
        {
            Debug.LogWarning("Target is null.");
        }

        UpdateCameraPosition();
    }

    void Update()
    {
        if (target == null) return;

        int touchCount = Input.touchCount;

        if (touchCount == 1)
        {
            HandleOneFinger(Input.GetTouch(0));
        }
        else if (touchCount == 2)
        {
            HandleTwoFingers(Input.GetTouch(0), Input.GetTouch(1));
        }
    }

    void HandleOneFinger(Touch touch)
    {
        // Double tap toggle pan mode
        if (touch.tapCount == 2 && Time.time - lastTapTime < doubleTapThreshold)
        {
            isPanningMode = !isPanningMode;
            Debug.Log("Double Tap - Toggle Pan Mode: " + isPanningMode);
            lastTapTime = 0f;
            return;
        }

        if (touch.phase == TouchPhase.Ended)
            lastTapTime = Time.time;

        if (isPanningMode)
        {
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                // Lấy trục di chuyển dựa vào view
                Vector3 right = cam.transform.right;
                Vector3 up = cam.transform.up;

                // Tính vector dịch chuyển (không có thành phần forward)
                Vector3 move = (-right * delta.x - up * delta.y) * panSpeed;

                // Di chuyển camera và pivotPoint theo move
                cam.transform.position += move;
                pivotPoint += move; // Quan trọng! Để giữ camera vẫn xoay quanh đúng điểm

                UpdateCameraPosition(); // Giữ nguyên góc nhìn
            }
        }
        else
        {
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPos = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastTouchPos;

                currentX += delta.x * rotationSpeed;
                currentY -= delta.y * rotationSpeed;
                currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

                UpdateCameraPosition();  // Cập nhật lại vị trí camera khi xoay
                lastTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }


    void HandleTwoFingers(Touch touch0, Touch touch1)
    {
        // Zoom
        float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
        float currentDistance = (touch0.position - touch1.position).magnitude;
        float zoomDelta = currentDistance - prevDistance;

        distance -= zoomDelta * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        cam.transform.position = pivotPoint + offset;
        cam.transform.LookAt(pivotPoint);
    }
}
