using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PDFMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject btnDownloadPDF;     // Nút Options ban đầu
    public GameObject printLayout;    // Panel menu chứa download PDF
    public GameObject menuOptions;    // Panel menu chứa các options
    public Button btnCancel;        // Nút đóng menu trong panel

    void Start()
    {
        if (btnDownloadPDF != null)
        {
            btnDownloadPDF.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowOptionsMenu();          // Mở menu
            });
        }

        if (btnCancel != null)
        {
            btnCancel.onClick.AddListener(HideOptionsMenu);
        }

        // Đảm bảo trạng thái ban đầu
        printLayout.SetActive(false);
        // menuOptions.SetActive(true);
        // btnDownloadPDF.SetActive(true);
    }

    void ShowOptionsMenu()
    {
        printLayout.SetActive(true);
        menuOptions.SetActive(false);
        btnDownloadPDF.SetActive(false);
    }

    void HideOptionsMenu()
    {
        printLayout.SetActive(false);
        menuOptions.SetActive(true);
        btnDownloadPDF.SetActive(true);
    }
}
