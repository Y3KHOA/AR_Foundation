using UnityEngine;
using UnityEngine.UI;

public class ButtonPanelManager : MonoBehaviour
{
    public GameObject panelPrefab; // Prefab của panel
    public Transform parentCanvas; // Canvas để chứa panel
    private GameObject currentPanel; // Lưu trữ panel hiện tại

    public void ShowPanel()
    {
        if (panelPrefab != null && parentCanvas != null)
        {
            // Kiểm tra nếu panel đã tồn tại thì không tạo thêm
            if (currentPanel == null)
            {
                currentPanel = Instantiate(panelPrefab, parentCanvas);
                currentPanel.transform.localPosition = Vector3.zero; // Hiển thị ở giữa màn hình
            }
            else
            {
                currentPanel.SetActive(true);
            }
        }
    }

    public void ClosePanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
    }
}
