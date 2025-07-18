using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputCreateRectangularRoom : MonoBehaviour
{
    [Header("UI References")]
    public GameObject sidesInputField;   // Số cạnh
    public GameObject lengthInputField;  // Chiều dài cạnh (m)
    public Button createButton;          // Nút "Tạo Room"
    public GameObject targetPanel; // Panel đóng
    [SerializeField] private ToastUI failedPopup;
    [Header("References")]
    public CheckpointManager checkpointManager; // Script vẽ

    void Start()
    {
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateRoomClicked);
        else
            Debug.LogError("Chưa gán CreateButton!");

        failedPopup.DescriptionText = "";
    }

    private const string WidthErrorLog = "Chiều rộng cạnh không hợp lệ! (>0)";
    private const string HeightErrorLog = "Chiều dài cạnh không hợp lệ! (>0)";

    void OnCreateRoomClicked()
    {
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager chưa gán!");
            return;
        }

        // === Lấy số cạnh ===
        int width = 0;
        // if (!int.TryParse(sidesInputField.text, out width) || width <= 0)
        TMP_InputField widthField = sidesInputField.GetComponentInChildren<TMP_InputField>();
        if (widthField == null || !int.TryParse(widthField.text, out width) || width <= 0)
        {
            Debug.LogWarning(WidthErrorLog);
            // PopupController.Show("Chiều dài cạnh không hợp lệ! (>0)", null);
            ShowInformationToast(WidthErrorLog);
            return;
        }

        // === Lấy chiều dài cạnh ===
        float height = 0f;
        // if (!float.TryParse(lengthInputField.text, out height) || height <= 0)
        TMP_InputField heightField = lengthInputField.GetComponentInChildren<TMP_InputField>();
        if (heightField == null || !float.TryParse(heightField.text, out height) || height <= 0)
        {
            Debug.LogWarning(HeightErrorLog);
            // PopupController.Show("Chiều rộng cạnh không hợp lệ! (>0)", null);
            ShowInformationToast(HeightErrorLog);
            return;
        }

        // === Truyền camera (nếu chưa gán sẵn) ===
        if (checkpointManager.drawingCamera == null)
            checkpointManager.drawingCamera = Camera.main;

        // === Tạo Room tự động ===
        checkpointManager.CreateRectangleRoom(width, height);

        Debug.Log($"[RoomShapeInputController] Gửi yêu cầu tạo Room hình chữ nhật chiều dài {width}m , cạnh rộng {width}m");
        targetPanel.SetActive(false);
        BackgroundUI.Instance.Hide();
    }

    private void ShowInformationToast(string descriptionText)
    {
        failedPopup.gameObject.SetActive(true);
        failedPopup.DescriptionText = HeightErrorLog;
    }
}