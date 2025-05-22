using UnityEngine;
using UnityEngine.UI;

public class ButtonCatalogPanel : MonoBehaviour
{
    [Header("Open Catalog Button")]
    public Button openCatalogButton;      // Nút để mở/đóng bảng danh mục (Catalog)
    public Sprite normalSprite;           // Hình ảnh nút khi bảng danh mục đang mở
    public Sprite closeSprite;            // Hình ảnh nút khi bảng danh mục đang đóng

    private GameManager gameManager;      // Tham chiếu đến singleton GameManager

    private void Start()
    {
        // Lấy instance của GameManager để truy cập guiCanvasManager
        gameManager = GameManager.instance;
    }

    // Hàm xử lý khi người dùng nhấn vào nút mở/đóng danh mục
    public void OpenCategoryOnClick()
    {
        // Nếu categoryCanvas đang hiện
        if (gameManager.guiCanvasManager.categoryCanvas.activeSelf == true)
        {
            // Đổi hình nút về trạng thái bình thường
            openCatalogButton.image.sprite = normalSprite;
        }
        else
        {
            // Đổi hình nút về trạng thái đóng
            openCatalogButton.image.sprite = closeSprite;
        }

        // Đảo trạng thái hiển thị của categoryCanvas (mở nếu đang đóng, đóng nếu đang mở)
        gameManager.guiCanvasManager.categoryCanvas.SetActive(!gameManager.guiCanvasManager.categoryCanvas.activeSelf);
    }
}
