using UnityEngine;
using UnityEngine.UI;

public class PanelManagerDoorWindow: MonoBehaviour
{
    public BtnController btnController; // Drag vào từ Inspector
    public GameObject panel;            // Drag Panel object vào
    public GameObject openButton;       // Drag nút mở Panel vào

    public Button BtnDoor;
    public Button BtnWindow;
    public Button BtnAdd;
    private void Start()
    {
        if (BtnDoor != null)
        {
            Debug.Log("BtnDoor assigned");
            BtnDoor.onClick.AddListener(ClosePanel);
        }

        if (BtnWindow != null)
        {
            Debug.Log("BtnWindow assigned");
            BtnWindow.onClick.AddListener(ClosePanel);
        }

        if (BtnAdd != null)
        {
            Debug.Log("BtnAdd assigned");
            BtnAdd.onClick.AddListener(ClosePanel);
        }
    }
    void Update()
    {
        if (btnController != null && btnController.Flag == 1)
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

        btnController.Flag = 0;
    }
    void ClosePanel()
    {
        Debug.Log("Button clicked - test");
        if (panel != null)
            panel.SetActive(false);
        if (openButton != null)
            openButton.SetActive(true);
    }
}
