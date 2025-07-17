using UnityEngine;
using UnityEngine.UI;

public class PanelToggleController : MonoBehaviour
{
    [Header("Toggle Elements")]
    public Button toggleButton; // Button để click

    public GameObject targetPanel; // Panel để mở/đóng

    private bool isPanelOpen = false;

    void Start()
    {
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
             BackgroundUI.Instance.Show(targetPanel, () =>
             {
                 TogglePanel();
                 BackgroundUI.Instance.Hide();
             });
            
        }
    }
}