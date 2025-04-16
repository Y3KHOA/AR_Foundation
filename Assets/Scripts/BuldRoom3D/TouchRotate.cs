using UnityEngine;
using UnityEngine.EventSystems;

public class TouchRotate : MonoBehaviour
{
    public Transform target;               // Đối tượng 3D cần xoay
    public float rotationSpeed = 0.2f;     // Tốc độ xoay X/Y
    public float zRotationSpeed = 0.4f;    // Tốc độ xoay Z
    public float orbitSpeed = 0.1f;         // Tốc độ xoay bằng 2 ngón
    public float panSpeed = 0.005f;         // Tốc độ di chuyển bằng 3 ngón
    public float zoomSpeed = 0.01f;

    private Vector2 lastTouchPos;
    private bool isDragging = false;
    private Vector3 modelCenter;

    private bool isPanningMode = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;
    float currentXRotation = 0f;
    const float minXRotation = -90f;
    const float maxXRotation = 90f;

    void Start()
    {
        if (target != null)
        {
            Renderer rend = target.GetComponentInChildren<Renderer>();
            if (rend != null)
                modelCenter = rend.bounds.center;
            else
                Debug.LogWarning("khong tim thay model");
        }
    }

    void Update()
    {
        if (target == null) return;
        int touchCount = Input.touchCount;

        if (touchCount == 0) return;

        // Kiểm tra từng touch có đang ở trên UI không
        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                // Nếu bất kỳ ngón nào chạm vào UI, thì bỏ qua xử lý touch
                return;
            }
        }

        if (touchCount == 1)
        {
            HandleOneFinger(Input.GetTouch(0));
        }
        else if (touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            HandleTwoFingers(touch0, touch1);
        }

        UpdateModelCenter();
    }

    void HandleOneFinger(Touch touch)
    {
        // Double tap toggle
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
                Vector3 move = new Vector3(delta.x, delta.y, 0) * panSpeed;
                Vector3 moveWorld = Camera.main.transform.TransformDirection(move);
                target.position += new Vector3(moveWorld.x, moveWorld.y, 0);
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
                float rotY = delta.x * rotationSpeed;
                float rotX = -delta.y * rotationSpeed;

                // Cập nhật góc xoay
                float newXRotation = currentXRotation + rotX;
                newXRotation = Mathf.Clamp(newXRotation, minXRotation, maxXRotation);

                currentXRotation = newXRotation;

                // Tính quaternion mới (Y xoay ngang, X xoay dọc, Z giữ nguyên)
                Quaternion newRotation = Quaternion.Euler(currentXRotation, target.eulerAngles.y + rotY, 0f);

                // Cập nhật rotation + giữ đúng vị trí xoay quanh modelCenter
                Vector3 offset = target.position - modelCenter;
                offset = newRotation * Quaternion.Inverse(target.rotation) * offset;

                target.rotation = newRotation;
                target.position = modelCenter + offset;

                UpdateModelCenter();

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
        // Orbit
        Vector2 avgDelta = (touch0.deltaPosition + touch1.deltaPosition) / 2f;
        float orbitY = avgDelta.x * orbitSpeed;
        float orbitX = -avgDelta.y * orbitSpeed;

        target.RotateAround(modelCenter, Vector3.up, orbitY);
        target.RotateAround(modelCenter, target.right, orbitX);

        UpdateModelCenter();

        // Zoom
        float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
        float currentDistance = (touch0.position - touch1.position).magnitude;
        float zoomDelta = prevDistance - currentDistance;

        // Zoom theo điểm giữa 2 ngón tay
        Vector2 midPoint = (touch0.position + touch1.position) / 2f;
        Ray ray = Camera.main.ScreenPointToRay(midPoint);

        Vector3 zoomOrigin;

        // Nếu ray chạm vào mô hình (có collider), lấy điểm chạm
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            zoomOrigin = hit.point;
        }
        else
        {
            // Nếu không chạm gì, lấy 1 điểm trên ray cách camera một khoảng nào đó (ví dụ 5 đơn vị)
            zoomOrigin = ray.origin + ray.direction * 5f;
        }

        Vector3 zoomDirection = (zoomOrigin - Camera.main.transform.position).normalized;
        target.position += zoomDirection * zoomDelta * zoomSpeed;
    }

    void UpdateModelCenter()
    {
        Renderer rend = target.GetComponentInChildren<Renderer>();
        if (rend != null)
            modelCenter = rend.bounds.center;
    }
}
