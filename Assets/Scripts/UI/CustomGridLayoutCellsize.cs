using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomGridLayoutCellsize : MonoBehaviour
{
    [Range(0f, 1f)] public float widthPercent = 0.9f;  // 90%
    [Range(0f, 1f)] public float heightPercent = 0.09f; // 9%
    public bool useScreenSize = false; // false = dùng kích thước RectTransform cha

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    private void Awake()
    {
        ScreenResizeWatcher.Instance.OnScreenResizeEvent += RunDelayFrames;
    }

    private void OnDestroy()
    {
        ScreenResizeWatcher.Instance.OnScreenResizeEvent -= RunDelayFrames;
    }

    void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        UpdateCellSize();
    }

    private void RunDelayFrames()
    {
        StartCoroutine(DelayFrames());
    }
    
    private IEnumerator DelayFrames()
    {
        yield return new WaitForEndOfFrame();
        UpdateCellSize();
    }
    

    private void OnEnable()
    {
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
        if (rectTransform == null || gridLayout == null) return;
        
        float width, height;

        if (useScreenSize)
        {
            width = Screen.width;
            height = Screen.height;
        }
        else
        {
            width = rectTransform.rect.width;
            height = rectTransform.rect.height;
        }

        float cellWidth = width * widthPercent;
        float cellHeight = height * heightPercent;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }
}