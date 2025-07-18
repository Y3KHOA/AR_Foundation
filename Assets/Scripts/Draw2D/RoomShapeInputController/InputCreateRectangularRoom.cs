using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputCreateRectangularRoom : MonoBehaviour
{
    [Header("UI References")]
    public GameObject lengthInputField;   // Chiều dài cạnh (m)
    public GameObject widthInputField;  // Chiều rộng cạnh (m)
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

        // Tự động focus vào chiều dài trước
        var field = lengthInputField.GetComponentInChildren<TMP_InputField>();
        if (field != null)
            field.Select();
    }

    void OnCreateRoomClicked()
    {
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager chưa gán!");
            return;
        }

        // === Lấy chiều dài ===
        float length = 0f;
        TMP_InputField lengthField = lengthInputField.GetComponentInChildren<TMP_InputField>();
        if (lengthField == null || !float.TryParse(lengthField.text, out length) || length <= 0)
        {
            Debug.LogWarning("Chiều dài không hợp lệ! (>0)");
            PopupController.Show("Chiều dài không hợp lệ! (>0)", null);
            return;
        }

        // === Lấy chiều rộng ===
        float width = 0f;
        TMP_InputField widthField = widthInputField.GetComponentInChildren<TMP_InputField>();
        if (widthField == null || !float.TryParse(widthField.text, out width) || width <= 0)
        {
            Debug.LogWarning("Chiều rộng không hợp lệ! (>0)");
            PopupController.Show("Chiều rộng không hợp lệ! (>0)", null);
            return;
        }

        // === Truyền camera (nếu chưa gán sẵn) ===
        if (checkpointManager.drawingCamera == null)
            checkpointManager.drawingCamera = Camera.main;

        // === Tạo Room hình chữ nhật ===
        checkpointManager.CreateRectangleRoom(length, width);

        Debug.Log($"[RoomShapeInputController] Gửi yêu cầu tạo Room hình chữ nhật chiều dài {length}m , cạnh rộng {width}m");
        targetPanel.SetActive(false);
    }
}