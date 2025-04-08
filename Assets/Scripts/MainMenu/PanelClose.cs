using UnityEngine;
using UnityEngine.UI;

public class PanelClose : MonoBehaviour
{
    public GameObject panel; // Gán Panel cần hiển thị/ẩn
    public Button closeButton; // Button để đóng Panel
    public Button button; // Kéo button vào Inspector nếu cần open 

    void Start()
    {
        // Kiểm tra nếu panel được gán vào, ẩn nó khi bắt đầu
        // if (panel != null)
        // {
        //     panel.SetActive(true); // Đảm bảo Panel ẩn khi bắt đầu
        // }

        // Kiểm tra nếu Button đóng được gán và thêm sự kiện
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel); // Gắn sự kiện nhấn nút
        }
    }

    // Hàm đóng Panel khi nhấn nút
    public void ClosePanel()
    {
        button.gameObject.SetActive(true); // Ẩn chính button này
        if (panel != null)
        {
            panel.SetActive(false); // Đóng Panel
        }
    }
}
