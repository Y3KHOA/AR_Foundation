using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HideParentPanel : MonoBehaviour, IPointerClickHandler
{
    [Header("Panel cần ẩn")]
    public GameObject panel;

    [Header("Nút Cancel")]
    public Button cancelButton;

    void Start()
    {
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() =>
            {
                HidePanel();
            });
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Kiểm tra nếu không click vào panel thì ẩn nó
        if (panel != null && !RectTransformUtility.RectangleContainsScreenPoint(
            panel.GetComponent<RectTransform>(), eventData.position, eventData.enterEventCamera))
        {
            HidePanel();
        }
    }

    private void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
        gameObject.SetActive(false); // ẩn lớp nền
    }
}
