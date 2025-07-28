using UnityEngine;
using UnityEngine.UI;

public class PanelToggleController : MonoBehaviour
{
    [Header("Toggle Elements")]
    public Button toggleButton; // Button để click

    public GameObject targetPanel; // Panel để mở/đóng

    private bool isPanelOpen = false;

    [SerializeField] private InputCreateRectangularRoom inputCreateRectangularRoom;

    void Start()
    {
        inputCreateRectangularRoom = GetComponent<InputCreateRectangularRoom>();
        
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (targetPanel != null)
            targetPanel.SetActive(false); // Ban đầu tắt panel
    }

    void TogglePanel()
    {
        if (targetPanel == null) return;

        isPanelOpen = !isPanelOpen;
        targetPanel.SetActive(isPanelOpen);

        Debug.Log(isPanelOpen ? "Panel Opened" : "Panel Closed");
        if (isPanelOpen)
        {
            inputCreateRectangularRoom.FocusOnInputField();
            BackgroundUI.Instance.Show(targetPanel, () =>
            {
                HideWhenOk();
            });
            
        }
    }

    public void HideWhenOk()
    {
        targetPanel.SetActive(false);
        isPanelOpen = false;
        BackgroundUI.Instance.Hide();
    }
}