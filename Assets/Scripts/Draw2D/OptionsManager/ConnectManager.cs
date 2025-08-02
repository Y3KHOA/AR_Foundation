using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ConnectManager : MonoBehaviour
{
    [SerializeField] public Button ConnectButton;
    public static bool isConnectActive = false;

    private CheckpointManager checkpointManager;
    private DrawingTool DrawTool;
    private PenManager penManager;
    private ToggleColorImage toggleColorImage;

    [SerializeField] private ToggleGroupUI toggleGroupUI;

    private GameObject selectedPoint = null;
    private GameObject selectedExtraCheckpoint = null;
    private GameObject selectedNormalCheckpoint = null;

    void Start()
    {
        isConnectActive = false;

        if (ConnectButton != null)
            ConnectButton.onClick.AddListener(ToggleConnect);

        toggleColorImage = ConnectButton?.GetComponent<ToggleColorImage>();
        if (toggleColorImage != null)
            toggleColorImage.Toggle(isConnectActive);

        checkpointManager = FindFirstObjectByType<CheckpointManager>();
        DrawTool = FindFirstObjectByType<DrawingTool>();
        penManager = FindFirstObjectByType<PenManager>();

        UpdateConnectState();
    }

    void Update()
    {
        if (!isConnectActive) return;

        if (Input.GetMouseButtonDown(0) && !IsClickingOnBackgroundBlackUI(Input.mousePosition))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitGO = hit.collider.gameObject;
                if (hitGO.CompareTag("Untagged") || hitGO.CompareTag("CheckpointExtra"))
                {
                    HandleCheckpointClick(hitGO);
                }
            }
        }
    }

    private readonly List<RaycastResult> raycastResults = new();

    private bool IsClickingOnBackgroundBlackUI(Vector2 screenPosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        raycastResults.Clear(); // Rất quan trọng
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        return raycastResults.Any(r => r.gameObject.name == "Background Black");
    }

    private void HandleCheckpointClick(GameObject checkpoint)
    {
        if (checkpoint == null) return;

        // Luôn gán selectedCheckpoint trong CheckpointManager nếu cần đồng bộ (tùy chọn)
        checkpointManager.selectedCheckpoint = checkpoint;

        if (selectedPoint == null)
        {
            selectedPoint = checkpoint;
            Debug.Log($"[Chọn điểm đầu tiên]: {checkpoint.name} ➜ {checkpoint.transform.position}");
        }
        else
        {
            if (selectedPoint != checkpoint)
            {
                checkpointManager?.ToggleConnectionBetweenCheckpoints(selectedPoint, checkpoint);
                Debug.Log($"[Kết nối] {selectedPoint.name} ↔ {checkpoint.name}");
            }
            else
            {
                Debug.Log($"[Huỷ chọn] Nhấn lại điểm cũ: {checkpoint.name}");
            }

            selectedPoint = null; // reset sau khi nối hoặc huỷ
        }
    }
    [SerializeField] private GameObject toastUI;
    void ToggleConnect()
    {
        bool newState = !isConnectActive;

        Debug.Log($"[ToggleConnect] Button clicked. New state = {newState}");

        // Nếu bật Connect và không có room ➜ không cho bật
        if (newState && (RoomStorage.rooms == null || RoomStorage.rooms.Count == 0))
        {
            Debug.LogWarning("[ToggleConnect] Không có room nào để bật kết nối.");
            // Show popup
            toastUI.gameObject.SetActive(true);
            return;
        }

        ChangeState(newState);
        HandleToggleGroupUI(newState);
    }

    private void HandleToggleGroupUI(bool state)
    {
        if (state)
        {
            toggleGroupUI.ToggleOffAll();
        }
        else
        {
            toggleGroupUI.ShowFirstButton();
        }
    }

    public void ChangeState(bool state)
    {
        Debug.Log($"[ToggleConnect] Đổi trạng thái Connect: {isConnectActive} --> {state}");

        isConnectActive = state;
        UpdateConnectState();
        toggleColorImage?.Toggle(isConnectActive);
    }

    void UpdateConnectState()
    {
        var tmpLabel = ConnectButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpLabel != null)
            tmpLabel.text = isConnectActive ? "Tắt nối" : "Bật nối";
    }
}
