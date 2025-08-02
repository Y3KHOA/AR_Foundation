using UnityEngine;
using UnityEngine.UI;

public class PanelToggleController : MonoBehaviour
{
    [Header("Toggle Elements")]
    public Button toggleButton; // Button để click

    public Button closeBtn;
    public GameObject targetPanel; // Panel để mở/đóng

    [SerializeField] private InputCreateRectangularRoom inputCreateRectangularRoom;

    void Start()
    {
        inputCreateRectangularRoom = GetComponent<InputCreateRectangularRoom>();
        
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (targetPanel != null)
            targetPanel.SetActive(false); // Ban đầu tắt panel

        if (closeBtn)
        {
            closeBtn.onClick.AddListener(() =>
            {
                Show(false);
            });
        }
    }

    void TogglePanel()
    {
        if (targetPanel == null) return;

        Show(true);
    }

    public void Show(bool isShow)
    {
        targetPanel.SetActive(isShow);
    }
}