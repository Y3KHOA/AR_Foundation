using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{

    public GameObject SuccessPanel; // Panel thông báo xuất thành công
    public GameObject ErrorPanel; // Panel thông báo lỗi xuất PDF

    public void OnSaveButtonClicked()
    {
        // TransData.Instance.TransferData(); // Lấy dữ liệu từ DataTransfer và lưu vào Máy
        try
        {
            SaveLoadManager.Save();   // Gọi Save để lưu ra file

            if (SuccessPanel != null)
                SuccessPanel.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Export PDF failed: " + ex.Message);

            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }

    public void OnLoadButtonClicked()
    {
        try
        {
            SaveLoadManager.Load(); // Gọi Load để lấy từ file và đổ ngược vào DataTransfer
            if (SuccessPanel != null)
                SuccessPanel.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Export PDF failed: " + ex.Message);

            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }
}
