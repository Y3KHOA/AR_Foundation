using UnityEngine;

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

        // Xử lý nhấn đúp (double tap)
        if (touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.tapCount == 2 && Time.time - lastTapTime < doubleTapThreshold)
            {
                isPanningMode = !isPanningMode;
                Debug.Log("Toggle Pan Mode: " + isPanningMode);
                lastTapTime = 0f; // reset để tránh nhảy nhiều lần
                return;
            }
            if (touch.phase == TouchPhase.Ended)
                lastTapTime = Time.time;
        }

        // --- PAN MODE ---
        if (touchCount == 1 && isPanningMode)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                Vector3 move = new Vector3(delta.x, delta.y, 0) * panSpeed;

                Vector3 moveWorld = Camera.main.transform.TransformDirection(move);
                target.position += new Vector3(moveWorld.x, moveWorld.y, 0);
            }
            return;
        }

        // --- ROTATE 1 FINGER ---
        if (touchCount == 1 && !isPanningMode)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPos = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 currentTouchPos = touch.position;
                Vector2 delta = currentTouchPos - lastTouchPos;

                float rotY = delta.x * rotationSpeed;   // Xoay trái/phải
                float rotX = -delta.y * rotationSpeed;  // Xoay lên/xuống

                // Chỉ xoay quanh trục Y và X
                target.RotateAround(modelCenter, Vector3.up, rotY);
                target.RotateAround(modelCenter, Camera.main.transform.right, rotX);

                lastTouchPos = currentTouchPos;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // --- ORBIT & ZOOM 2 FINGERS ---
        if (touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Orbit
            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                Vector2 avgDelta = (touch0.deltaPosition + touch1.deltaPosition) / 2f;
                float orbitY = avgDelta.x * orbitSpeed;
                float orbitX = -avgDelta.y * orbitSpeed;

                target.RotateAround(modelCenter, Vector3.up, orbitY);
                target.RotateAround(modelCenter, target.right, orbitX);
            }

            // Zoom (pinch)
            float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
            float currentDistance = (touch0.position - touch1.position).magnitude;
            float zoomDelta = currentDistance - prevDistance;

            Vector3 zoomDirection = (Camera.main.transform.position - target.position).normalized;
            target.position += zoomDirection * zoomDelta * zoomSpeed;
        }
    }
}
