using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPan2D : MonoBehaviour
{
    public float zoomSpeed = 0.01f;
    public float panSpeed = 0.01f;

    [Header("PreviewCamera")]
    public Camera PreviewCamera;

    private bool isPanningMode = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    private Vector2 lastTouchPos;

    void Update()
    {
        if (PreviewCamera == null) return;
        int touchCount = Input.touchCount;

        if (touchCount == 0) return;

        // Bỏ qua nếu touch trên UI
        for (int i = 0; i < touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return;
        }

        if (touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            HandleOneFinger(touch);
        }
        else if (touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            HandleTwoFingers(touch0, touch1);
        }
    }

    void HandleOneFinger(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
            lastTouchPos = touch.position;

        if (touch.phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.position - lastTouchPos;

            // Di chuyển camera theo mặt phẳng xz (x: trái/phải, z: lên/xuống)
            Vector3 move = new Vector3(-delta.x * panSpeed, 0, -delta.y * panSpeed);
            PreviewCamera.transform.Translate(move, Space.World);

            lastTouchPos = touch.position;
        }
    }

    void HandleTwoFingers(Touch touch0, Touch touch1)
    {
        float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
        float currentDistance = (touch0.position - touch1.position).magnitude;
        float zoomDelta = currentDistance - prevDistance;

        // Zoom bằng cách thay đổi position Y
        Vector3 camPos = PreviewCamera.transform.position;
        camPos.y -= zoomDelta * zoomSpeed;
        camPos.y = Mathf.Clamp(camPos.y, 1f, 100f); // clamp tránh lật trục hoặc quá gần
        PreviewCamera.transform.position = camPos;

        Debug.Log($"[TouchPan2D] Zoom delta: {zoomDelta}");
    }
}
