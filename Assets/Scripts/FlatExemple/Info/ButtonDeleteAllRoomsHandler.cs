using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonDeleteAllRoomsHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelDeleteConfirm;  // Panel xác nhận xoá
    public GameObject panelError;          // Panel báo lỗi nếu có
    public GameObject panelSuccess;        // Panel báo xoá thành công

    // Gọi khi nhấn nút Delete (bật panel xác nhận)

    private const string PopupWarning = "BẠN THỰC SỰ MUỐN XÓA TOÀN BỘ DỮ LIỆU CHỨ? \n DỮ LIỆU XÓA KHÔNG THỂ KHÔI PHỤC!";
    
    public void OnClickDelete()
    {
        panelDeleteConfirm.gameObject.SetActive(true);
        // var popup = Instantiate(ModularPopup.Prefab);
        // popup.AutoFindCanvasAndSetup();
        // popup.Header = PopupWarning;
        // popup.ClickYesEvent = OnConfirmDelete;
        // popup.ClickNoEvent = OnCancelDelete;
        // popup.EventWhenClickButtons = () =>
        // {
        //     BackgroundUI.Instance.Hide();
        // };
        // popup.autoClearWhenClick = true;
        // BackgroundUI.Instance.Show(popup.gameObject,null);
    }

    // Gọi khi nhấn "Xác nhận" trong panel
    public void OnConfirmDelete()
    {
        try
        {
            // Xóa dữ liệu trong RoomStorage
            RoomStorage.rooms.Clear();
            Debug.Log("[Delete] Delete complete in RoomStorage");

            // Ẩn panel xác nhận
            if (panelDeleteConfirm != null)
                panelDeleteConfirm.SetActive(false);

            // Hiện panel thành công
            if (panelSuccess != null)
                panelSuccess.SetActive(true);

            // Reload lại scene
            SceneManager.LoadScene("FlatExampleScene");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Xoá thất bại: " + ex.Message);
            if (panelError != null)
                panelError.SetActive(true);
        }
    }

    // Gọi khi nhấn "Huỷ"
    public void OnCancelDelete()
    {
        
    }
}
