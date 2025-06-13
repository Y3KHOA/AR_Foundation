using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    public GameObject SuccessPanel; // Panel thông báo xuất thành công
    public GameObject ErrorPanel; // Panel thông báo lỗi xuất PDF

    [Header("UI Panels Save")]
    public GameObject PanelSaveFile;
    public GameObject PnaelView;

    [Header("UI Panels Load")]
    public GameObject PanelLoadFile;

    public void OnSaveButtonClicked()
    {
        try
        {
            // Mở panel nhập tên file
            if (PanelSaveFile != null)
                PanelSaveFile.SetActive(true);

            // Ẩn panel chính
            if (PnaelView != null)
                PnaelView.SetActive(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("UI toggle failed: " + ex.Message);

            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }

    public void OnLoadButtonClicked()
    {
        try
        {
            // Mở panel nhập tên file
            if (PanelLoadFile != null)
                PanelLoadFile.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("UI toggle failed: " + ex.Message);

            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }
}
