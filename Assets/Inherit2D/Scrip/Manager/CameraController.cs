using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    private GameManager gameManager;

    [Header("Speed")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float zoomSpeed = 3f;
    private float currentZoom = 10f;

    [Header("Clamp")]
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 150f;
    [SerializeField] private float minX = -170f;
    [SerializeField] private float maxX = 170f;
    [SerializeField] private float minY = -90f;
    [SerializeField] private float maxY = 90f;

    private Camera mainCamera;
    private bool isZooming = false;
    private bool justZoomed = false;
    private float zoomCooldown = 0.1f;
    private float zoomCooldownTimer = 0f;
    private Vector3 currentVelocity;
    private float smoothDeltaMagnitudeDiff = 0f;
    private float zoomLerpSpeed = 10f;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private float minVerticalAngle = -90f; // 🔹 Giới hạn thấp nhất
    private float maxVerticalAngle = 90f;  // 🔹 Giới hạn cao nhất
    private float currentVerticalAngle = 0f; // 🔹 Theo dõi góc hiện tại

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        mainCamera = Camera.main;
        currentZoom = mainCamera.orthographicSize;
    }

    private void Update()
    {
        if (gameManager.isLock) return;

        if (justZoomed)
        {
            zoomCooldownTimer -= Time.deltaTime;
            if (zoomCooldownTimer <= 0)
            {
                justZoomed = false;
            }
        }

        if (Input.touchCount > 0 && gameManager.guiCanvasManager.isOnWordSpace)
        {
            CaculateSpeed();
            HandleTouchZoom();
        }
        if (!justZoomed && !isZooming && Input.GetMouseButton(0) && gameManager.guiCanvasManager.isOnWordSpace && !gameManager.hasItem && !gameManager.GetDrawingStatus())
        {
            CaculateSpeed();
            if(!gameManager.isOn3DView)
            {
                HandleMouseMovement();
            }    
            else
            {
                HandleTouchRotation3D();
                HandleTouchMovement3D();
            }    
        }
    }

    private void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (!isZooming)
            {
                isZooming = true;
                return;
            }

            if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
            {
                float currentTouchDelta = (touch0.position - touch1.position).magnitude;
                float previousTouchDelta = ((touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition)).magnitude;

                float deltaMagnitudeDiff = previousTouchDelta - currentTouchDelta;
                smoothDeltaMagnitudeDiff = Mathf.Lerp(smoothDeltaMagnitudeDiff, deltaMagnitudeDiff, Time.deltaTime * zoomLerpSpeed);

                currentZoom -= smoothDeltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
                currentZoom = Mathf.Clamp(currentZoom, minScale, maxScale);

                mainCamera.orthographicSize = currentZoom;
            }

            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isZooming = false;
                justZoomed = true;
                zoomCooldownTimer = zoomCooldown;
                smoothDeltaMagnitudeDiff = 0;
            }
        }
        else
        {
            isZooming = false;
        }
    }

    private void HandleMouseMovement()
    {
        Vector3 mouseDelta = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0) * moveSpeed * Time.deltaTime;

        if (mouseDelta.magnitude > 0.01f)
        {
            Vector3 targetPosition = mainCamera.transform.position + mouseDelta;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref currentVelocity, 0.1f);
        }
    }

    private void CaculateSpeed()
    {
        moveSpeed = mainCamera.orthographicSize / 1.6f;
        zoomSpeed = mainCamera.orthographicSize / 30;
    }

    private void HandleTouchRotation3D()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.deltaPosition * 0.02f; // 🔹 Giảm tốc độ xoay

                float rotationX = delta.x * moveSpeed * Time.deltaTime;
                float rotationY = -delta.y * moveSpeed * Time.deltaTime;

                // 🔹 Xoay quanh trục Y (ngang) không giới hạn
                mainCamera.transform.Rotate(Vector3.up, rotationX, Space.World);

                // 🔹 Lấy góc hiện tại của camera
                float newVerticalAngle = currentVerticalAngle + rotationY;

                // 🔹 Clamp trong khoảng min-max
                newVerticalAngle = Mathf.Clamp(newVerticalAngle, minVerticalAngle, maxVerticalAngle);

                // 🔹 Áp dụng xoay mới (giữ nguyên trục X & Z)
                mainCamera.transform.localRotation = Quaternion.Euler(newVerticalAngle, mainCamera.transform.eulerAngles.y, 0);

                // 🔹 Cập nhật góc hiện tại
                currentVerticalAngle = newVerticalAngle;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
    }

    private void HandleTouchMovement3D()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // 🔹 Lấy trung bình deltaPosition của cả hai ngón tay
            Vector2 avgDelta = (touch0.deltaPosition + touch1.deltaPosition) / 2;

            // 🔹 Chuyển đổi sang Vector3 để di chuyển trong không gian 3D
            Vector3 moveDirection = -mainCamera.transform.right * avgDelta.x + -mainCamera.transform.up * avgDelta.y;
            moveDirection *= moveSpeed * Time.deltaTime * 0.5f;

            // 🔹 Cập nhật vị trí camera
            Vector3 targetPosition = mainCamera.transform.position + moveDirection;

            // 🔹 Clamp vị trí nếu cần (giới hạn phạm vi di chuyển)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref currentVelocity, 0.1f);
        }
    }
}