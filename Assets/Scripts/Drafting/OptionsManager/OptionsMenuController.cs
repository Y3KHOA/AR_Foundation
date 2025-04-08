using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OptionsMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject btnOptions;     // Nút Options ban đầu
    public GameObject menuOptions;    // Panel menu chứa các options
    public GameObject downloadPDF;       // Nút tải xuống PDF
    public Button closeButton;        // Nút đóng menu trong panel

    void Start()
    {
        if (btnOptions != null)
        {
            btnOptions.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowOptionsMenu();          // Mở menu
            });
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideOptionsMenu);
        }

        // Đảm bảo trạng thái ban đầu
        menuOptions.SetActive(false);
        downloadPDF.SetActive(false);
        btnOptions.SetActive(true);
    }

    void ShowOptionsMenu()
    {
        menuOptions.SetActive(true);
        downloadPDF.SetActive(true);
        btnOptions.SetActive(false);
    }

    void HideOptionsMenu()
    {
        menuOptions.SetActive(false);
        downloadPDF.SetActive(false);
        btnOptions.SetActive(true);
    }
}
