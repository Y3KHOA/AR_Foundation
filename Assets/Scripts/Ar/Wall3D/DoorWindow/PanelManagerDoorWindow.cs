using UnityEngine;
using UnityEngine.UI;

public class PanelManagerDoorWindow : MonoBehaviour
{
    public BtnController btnController; // Drag vào từ Inspector
    public GameObject panel;            // Drag Panel object vào
    public GameObject openButton;       // Drag nút mở Panel vào

    public Button BtnDoor;
    public Button BtnWindow;
    public Button BtnAdd;

    bool isClicked = true;

    public static PanelManagerDoorWindow Instance { get; private set; }
    private bool isDoorChanged;  // Biến flag để theo dõi sự thay đổi của dữ liệu Door
    private bool isWindowChanged;  // Biến flag để theo dõi sự thay đổi của dữ liệu Window


    void Awake()
    {
        // Gán Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Nếu đã có một instance khác => hủy
        }
        else
        {
            Instance = this;
        }
        isDoorChanged = false;  // Khởi tạo mặc định là chưa thay đổi
        isWindowChanged = false;  // Khởi tạo mặc định là chưa thay đổi
    }
    private void Start()
    {
        BtnDoor.onClick.AddListener(() =>
        {
            isDoorChanged = true;
            isWindowChanged = false;
            ClosePanel();
        });

        BtnWindow.onClick.AddListener(() =>
        {
            isDoorChanged = false;
            isWindowChanged = true;
            ClosePanel();
        });

        if (BtnAdd != null)
        {
            Debug.Log("BtnAdd assigned");
            BtnAdd.onClick.AddListener(ClosePanel);
        }
    }
    void Update()
    {
        if (btnController != null && btnController.Flag == 1 && isClicked)
        {
            ShowPanel();
        }
    }

    private void ShowPanel()
    {
        if (panel != null)
            panel.SetActive(true);

        if (openButton != null)
            openButton.SetActive(false);

        isClicked = false;
    }
    void ClosePanel()
    {
        Debug.Log("Button clicked - test");
        if (panel != null)
            panel.SetActive(false);
        if (openButton != null)
            openButton.SetActive(true);
    }

    public bool IsDoorChanged
    {
        get => isDoorChanged;
        set => isDoorChanged = value;
    }

    public bool IsWindowChanged
    {
        get => isWindowChanged;
        set => isWindowChanged = value;
    }

    public bool IsClicked
    {
        get => isClicked;
        set => isClicked = value;
    }

}
