using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    public TransData transData;

    public void OnSaveButtonClicked()
    {
        transData.TransferData(); // Lấy dữ liệu từ BtnController và lưu vào DataTransfer
        SaveLoadManager.Save();   // Gọi Save để lưu ra file
    }

    public void OnLoadButtonClicked()
    {
        SaveLoadManager.Load(); // Gọi Load để lấy từ file và đổ ngược vào DataTransfer
    }
}
