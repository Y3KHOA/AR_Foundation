using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class InputCreateRectangularRoom : MonoBehaviour
{
    [Header("UI References")]
    public GameObject lengthInputField;   // Chiều dài cạnh (m)
    public GameObject widthInputField;  // Chiều rộng cạnh (m)
    public Button createButton;          // Nút "Tạo Room"
    public GameObject targetPanel; // Panel đóng
    [SerializeField] private ToastUI failedPopup;
    [Header("References")]
    public CheckpointManager checkpointManager; // Script vẽ

    [SerializeField] private PanelToggleController panelToggleController;


    private TMP_InputField _lengthInputField;
    private TMP_InputField _widthInputField;

    private const int LIMIT_CHARACTER_COUNT = 9;
    

    private void Awake()
    {
        _lengthInputField = lengthInputField.GetComponentInChildren<TMP_InputField>();
        _widthInputField = widthInputField.GetComponentInChildren<TMP_InputField>();
        
        _lengthInputField.characterLimit = LIMIT_CHARACTER_COUNT;
        _widthInputField.characterLimit = LIMIT_CHARACTER_COUNT;
    }

    void Start()
    {
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateRoomClicked);
        else
            Debug.LogError("Chưa gán CreateButton!");

        failedPopup.DescriptionText = "";
        panelToggleController = GetComponent<PanelToggleController>();
        // Tự động focus vào chiều dài sau 1 frame
    }

    public void FocusOnInputField()
    {
        Debug.Log("Da goi method focus khi bật lên");
        if (string.IsNullOrWhiteSpace(_lengthInputField.text))
        {
            StartCoroutine(FocusLengthInputNextFrame(_lengthInputField));
            return;
        }
        if (string.IsNullOrWhiteSpace(_widthInputField.text))
        {
            StartCoroutine(FocusLengthInputNextFrame(_widthInputField));
            return;
        }
    }

    private IEnumerator FocusLengthInputNextFrame(TMP_InputField inputField)
    {
        yield return null; // Đợi 1 frame

        if (inputField != null)
        {
            // Set selected game object để Unity UI focus đúng
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.OnPointerClick(new PointerEventData(EventSystem.current)); // kích hoạt caret
        }
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

        // === Lấy chiều dài ===
        float length = 0f;
        TMP_InputField lengthField = lengthInputField.GetComponentInChildren<TMP_InputField>();
        if (lengthField == null || !float.TryParse(lengthField.text, out length) || length <= 0)
        {
            Debug.LogWarning(WidthErrorLog);
            // PopupController.Show("Chiều dài cạnh không hợp lệ! (>0)", null);
            ShowInformationToast(WidthErrorLog);
            return;
        }

        // === Lấy chiều rộng ===
        float width = 0f;
        TMP_InputField widthField = widthInputField.GetComponentInChildren<TMP_InputField>();
        if (widthField == null || !float.TryParse(widthField.text, out width) || width <= 0)
        {
            Debug.LogWarning(HeightErrorLog);
            // PopupController.Show("Chiều rộng cạnh không hợp lệ! (>0)", null);
            ShowInformationToast(HeightErrorLog);
            return;
        }

        // === Truyền camera (nếu chưa gán sẵn) ===
        if (checkpointManager.drawingCamera == null)
            checkpointManager.drawingCamera = Camera.main;

        // === Tạo Room hình chữ nhật ===
        checkpointManager.CreateRectangleRoom(length, width);

        Debug.Log($"[RoomShapeInputController] Gửi yêu cầu tạo Room hình chữ nhật chiều dài {length}m , cạnh rộng {width}m");
        panelToggleController.Show(false);
    }

    private void ShowInformationToast(string descriptionText)
    {
        failedPopup.gameObject.SetActive(true);
        failedPopup.DescriptionText = HeightErrorLog;
    }
}