using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPan2D : MonoBehaviour
{
    public Transform modelRoot;
    public float zoomSpeed = 0.01f;
    public float panSpeed = 0.005f;

    private bool isPanningMode = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    private Vector2 lastTouchPos;

    void Update()
    {
        if (modelRoot == null) return;
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
        // Double tap toggle Pan
        if (touch.tapCount == 2 && Time.time - lastTapTime < doubleTapThreshold)
        {
            isPanningMode = !isPanningMode;
            Debug.Log($"[TouchPan2D] Double Tap: PanMode={isPanningMode}");
            lastTapTime = 0f;
            return;
        }

        if (touch.phase == TouchPhase.Ended)
            lastTapTime = Time.time;

        if (isPanningMode)
        {
            if (touch.phase == TouchPhase.Began)
                lastTouchPos = touch.position;

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouchPos;
                Vector3 move = new Vector3(delta.x, 0, delta.y) * panSpeed;
                modelRoot.Translate(move, Space.World);
                lastTouchPos = touch.position;
            }
        }
    }

    void HandleTwoFingers(Touch touch0, Touch touch1)
    {
        float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
        float currentDistance = (touch0.position - touch1.position).magnitude;
        float zoomDelta = currentDistance - prevDistance;

        Vector3 scaleChange = Vector3.one * (1f + zoomDelta * zoomSpeed);
        modelRoot.localScale = Vector3.Scale(modelRoot.localScale, scaleChange);

        Debug.Log($"[TouchPan2D] Zoom delta: {zoomDelta}");
    }
}
