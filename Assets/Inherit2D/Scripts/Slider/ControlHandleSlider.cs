using UnityEngine;

/// <summary>
/// Lớp đại diện cho vật liệu mặt đất trong trò chơi.
/// </summary>
public class NewMonoBehaviourScript : MonoBehaviour
{
    public RectTransform handleRect;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        ControlSize();
    }

    private void ControlSize()
    {
        float parentHeight = rectTransform.rect.height;
        handleRect.sizeDelta = new Vector2(parentHeight * 0.85f, handleRect.sizeDelta.y);

        // Đặt khoảng cách từ dưới (Bottom)
        handleRect.offsetMin = new Vector2(handleRect.offsetMin.x, parentHeight * 0.05f);

        // Đặt khoảng cách từ trên (Top)
        handleRect.offsetMax = new Vector2(handleRect.offsetMax.x, -parentHeight * 0.05f);
    }
}
