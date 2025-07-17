using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputCreatePolygonRoom : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField sidesInputField;   // Số cạnh
    public TMP_InputField lengthInputField;  // Chiều dài cạnh (m)
    public Button createButton;          // Nút "Tạo Room"
    public GameObject targetPanel; // Panel đóng

    [Header("References")]
    public CheckpointManager checkpointManager; // Script vẽ

    void Start()
    {
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateRoomClicked);
        else
            Debug.LogError("Chưa gán CreateButton!");
    }

    void OnCreateRoomClicked()
    {
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager chưa gán!");
            return;
        }

        // === Lấy số cạnh ===
        int sides = 0;
        if (!int.TryParse(sidesInputField.text, out sides) || sides < 3)
        {
            Debug.LogWarning("Số cạnh không hợp lệ! (>=3)");
            PopupController.Show("Số cạnh không hợp lệ! (>=3)", null);
            return;
        }

        // === Lấy chiều dài cạnh ===
        float length = 0f;
        if (!float.TryParse(lengthInputField.text, out length) || length <= 0)
        {
            Debug.LogWarning("Chiều dài cạnh không hợp lệ! (>0)");
            PopupController.Show("Chiều dài cạnh không hợp lệ! (>0)", null);
            return;
        }

        // === Truyền camera (nếu chưa gán sẵn) ===
        if (checkpointManager.drawingCamera == null)
            checkpointManager.drawingCamera = Camera.main;

        // === Tạo Room tự động ===
        checkpointManager.CreateRegularPolygonRoom(sides, length);

        Debug.Log($"[RoomShapeInputController] Gửi yêu cầu tạo Room {sides} cạnh, cạnh dài {length}m");
        targetPanel.SetActive(false);
    }
}
