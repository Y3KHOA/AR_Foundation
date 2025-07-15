using UnityEngine;

public class AutoContentSizeControl : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private float totalWidth, totalHeigh;
    private int previousChildCount = -1;

    private void LateUpdate()
    {
        int childCount = 0;
        foreach (Transform item in rectTransform.transform)
        {
            if(item.gameObject.activeSelf)
                childCount += 1;
        }

        if (childCount != previousChildCount)
        {
            totalWidth = 0;
            totalHeigh = 0;
            foreach (RectTransform item in rectTransform.transform)
            {
                Debug.Log("Plus: " + item.rect.width);
                totalWidth += item.rect.width;
                totalWidth += item.rect.height;
            }

            Debug.Log($"$Current Size Delta {rectTransform.rect.width}");
            Debug.Log($"Total width {totalWidth}");
            Debug.Log($"Total heigh {totalHeigh}");
            previousChildCount = childCount;
            rectTransform.sizeDelta = new Vector2(totalWidth / 1.7f, rectTransform.sizeDelta.y);
        }
    }
}