using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PenManager : MonoBehaviour
{
    public Button penButton; // Button để bật/tắt chức năng pen
    public static Camera mainCamera; // Camera chính để di chuyển và phóng to
    public float zoomSpeed = 2f; // Tốc độ zoom
    public float panSpeed = 0.5f; // Tốc độ di chuyển
    public static bool isRoomFloorBeingDragged = false;

    public GameObject ActionSpace;

    public static bool isPenActive = true; // Trạng thái của Pen (bật/tắt)
    private CheckpointManager checkpointManager; // Tham chiếu đến CheckpointManager để điều khiển vẽ
    private DrawingTool DrawTool; // Tham chiếu đến DrawingTool để điều khiển vẽ
    private bool isTouchStartedInActionSpace = false;


    private ToggleColorImage toggleColorImage;

    // public bool IsPenActive => isPenActive;  // Getter để cung cấp trạng thái Pen
    private Vector3 previewPosition; // Vị trí preview
    [SerializeField] private ToggleGroupUI toggleGroupUI;

    [SerializeField] DrawingTool drawingTool;

    void Start()
    {
        isPenActive = true;
        mainCamera = Camera.main;
        // Gán sự kiện click vào Button
        penButton.onClick.AddListener(TogglePen);
        toggleColorImage = penButton.GetComponent<ToggleColorImage>();
        toggleColorImage.Toggle(isPenActive);
        // Lấy tham chiếu đến CheckpointManager
        checkpointManager = FindFirstObjectByType<CheckpointManager>();

        // // Tự gán Collider nếu chưa có
        // GameObject bg = GameObject.Find("Background Black");
        // if (bg != null && bg.GetComponent<Collider>() == null)
        // {
        //     bg.AddComponent<BoxCollider>();
        // }

        // Đảm bảo trạng thái ban đầu của Pen là tắt
        UpdatePenState();
        HandleToggleGroupUI(isPenActive);
        DrawTool = FindFirstObjectByType<DrawingTool>();
    }

    void Update()
    {
        if (ConnectManager.isConnectActive) return;// đang nối line dừng mọi move với pen

        if (!isPenActive)
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
                // Debug.Log("RoomFloor drag đang hoạt động ➜ Khóa pan/zoom");
                HandleZoomAndPan(false);
            }
            else
            {
                HandleZoomAndPan(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                checkpointManager.SelectCheckpoint();
            }
            else if (Input.GetMouseButton(0))
            {
                // if (checkpointManager.IsInSavedLoop(checkpointManager.selectedCheckpoint) || checkpointManager.isClosedLoop)
                if (checkpointManager.selectedCheckpoint != null)
                {
                    checkpointManager.isMovingCheckpoint = true;
                    checkpointManager.MoveSelectedCheckpoint();
                    checkpointManager.isDragging = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                checkpointManager.DeselectCheckpoint();
                checkpointManager.isDragging = false;
                checkpointManager.isMovingCheckpoint = false;
            }
        }
    }

    private bool IsPointerInActionSpace(Vector2 screenPosition)
    {
        if (ActionSpace == null) return false;

        RectTransform rt = ActionSpace.GetComponent<RectTransform>();

        // Nếu canvas là Overlay, camera nên là null
        Canvas canvas = ActionSpace.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : mainCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition, cam);
    }

    private bool IsClickingOnBackgroundBlackUI(Vector2 screenPosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == "Background Black")
            {
                Debug.Log("Click UI trên Background Black ➜ Không cho pan/zoom");
                return true;
            }
        }

        return false;
    }

    // Hàm xử lý zoom và di chuyển camera
    public void HandleZoomAndPan(bool canZoomAndPan)
    {
        if (!canZoomAndPan) return;

        // kiểm tra thao tác bắt đầu trong ActionSpace
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            isTouchStartedInActionSpace = IsPointerInActionSpace(Input.mousePosition);

            if (IsClickingOnBackgroundBlackUI(Input.mousePosition))
                isTouchStartedInActionSpace = false;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            isTouchStartedInActionSpace = IsPointerInActionSpace(Input.GetTouch(0).position);

            if (IsClickingOnBackgroundBlackUI(Input.GetTouch(0).position))
                isTouchStartedInActionSpace = false;
        }
#endif
        if (!isTouchStartedInActionSpace) return;

        // if (checkpointManager != null && checkpointManager.isMovingCheckpoint)
        // {
        //     // Debug.Log("Đang move checkpoint ➜ KHÔNG pan/zoom!");
        //     return;
        // }

        if (checkpointManager != null && checkpointManager.selectedCheckpoint != null && checkpointManager.isDragging)
        {
            Debug.Log("Đang move checkpoint ➜ KHÔNG pan/zoom!");
            return;
        }

        if (IsTouchOverRoomFloor())
        {
            Debug.Log("Đang chạm   ➜ KHÔNG zoom/pan!");
            return;
        }

        // Thêm kiểm tra raycast vào đầu tiên
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.name == "Background Black")
                    {
                        Debug.Log("Raycast hit Background Black ➜ Không cho pan/zoom");
                        return;
                    }

                    if (hit.collider.gameObject.CompareTag("RoomFloor"))
                    {
                        Debug.Log("Raycast đang hit RoomFloor ➜ Bỏ pan/zoom bàn cờ!");
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
        Debug.Log("Is room Floor is move" + isRoomFloorBeingDragged);
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 touchDelta = touch.deltaPosition;
                Vector3 move =
                    mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y,
                        mainCamera.nearClipPlane)) -
                    mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x - touchDelta.x,
                        touch.position.y - touchDelta.y, mainCamera.nearClipPlane));

                mainCamera.transform.Translate(-move, Space.World);
            }
        }

        // Phóng to/thu nhỏ bằng hai ngón tay (pinch-to-zoom)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float prevMagnitude = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition))
                .magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            mainCamera.orthographicSize =
                Mathf.Clamp(mainCamera.orthographicSize - difference * zoomSpeed * 0.01f, 1f, 70f);
        }
    }

    // Toggle trạng thái Pen
    void TogglePen()
    {
        var newActivePenState = !isPenActive;

        if (!newActivePenState && RoomStorage.rooms.Count == 0)
        {
            // Show popup
            return;
        }

        ChangeState(newActivePenState);
        HandleToggleGroupUI(newActivePenState);
    }

    private void HandleToggleGroupUI(bool currentPenState)
    {
        if (ConnectManager.isConnectActive) return;
        if (currentPenState)
        {
            // tắt hết toggle bên kia
            toggleGroupUI.ToggleOffAll();
        }
        else
        {
            toggleGroupUI.ShowFirstButton();
            // bật button đầu tiên
        }
    }

    public void ChangeState(bool state)
    {
        isPenActive = state; // Chuyển trạng thái Pen
        UpdatePenState(); // Cập nhật trạng thái Pen
        toggleColorImage.Toggle(isPenActive);
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
            return Physics.Raycast(ray, out RaycastHit hit) &&
                   hit.collider.gameObject.layer == LayerMask.NameToLayer("RoomFloor");
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