using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý các danh mục vật phẩm trong trò chơi, cho phép tải và hiển thị dữ liệu danh mục một cách động.
/// </summary>
public class CategoryController : MonoBehaviour
{
    public GameObject categoryCanvasPrefab;

    private GameManager gameManager;
    private ItemsController itemsController;
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;
    private ScrollRect scrollRect;
    [HideInInspector] public List<CategoryCanvas> categoryCanvasList = new List<CategoryCanvas>();
    private List<TextMeshProUGUI> textMeshProUGUIsList = new List<TextMeshProUGUI>();
    private float cellSizeX;
    private float cellSizeY;
    private float adj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemsController = ItemsController.instance;
        LoadCategoryOnce();
        textMeshProUGUIsList = GetComponentsInChildren<TextMeshProUGUI>().ToList();

        //Adjust
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponentInParent<ScrollRect>();

        ControllItemsBoxCellSize();
        if (rectTransform != null)
        {
            UpdateBottom();

            //Điều chỉnh top
            Vector2 offsetMax = rectTransform.offsetMax; //Top
            Vector2 offsetMin = rectTransform.offsetMin; //Bottom
            offsetMin.y = -adj - offsetMax.y;
            offsetMax.y = 0;
            rectTransform.offsetMax = offsetMax;
            rectTransform.offsetMin = offsetMin;
        }

        // Đặt lại vị trí cuộn về đầu
        scrollRect.verticalNormalizedPosition = 1.0f;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (scrollRect != null && rectTransform != null)
        {
            // Lưu vị trí cuộn hiện tại dựa trên tỉ lệ
            float currentNormalizedPosition = scrollRect.verticalNormalizedPosition;

            // Cập nhật cấu trúc layout
            ControllItemsBoxCellSize();
            UpdateBottom();

            // Khôi phục vị trí cuộn
            scrollRect.verticalNormalizedPosition = currentNormalizedPosition;
        }
    }

    private void ControllItemsBoxCellSize()
    {
        if (gridLayoutGroup == null) return;
        float witdh = rectTransform.rect.width;
        cellSizeX = witdh * 0.87f;
        cellSizeY = witdh * 0.25f;
        gridLayoutGroup.cellSize = new Vector2(cellSizeX, cellSizeY);

        float spacingY = (rectTransform.rect.height / 100) * 1.2f;
        gridLayoutGroup.spacing = new Vector2(0, spacingY);

        //Font
        ControlFontSize();
    }

    private void UpdateBottom()
    {
        if (gridLayoutGroup == null || itemsController.categoryList.Count == 0) return;

        float contentHeight = gridLayoutGroup.cellSize.y * Mathf.Ceil(itemsController.categoryList.Count / (float)gridLayoutGroup.constraintCount);

        // Đảm bảo rằng chiều cao này không nhỏ hơn chiều cao của viewport
        float viewportHeight = scrollRect.viewport.rect.height;
        float adjustedContentHeight = Mathf.Max(contentHeight, viewportHeight);
        adjustedContentHeight -= viewportHeight / 4;

        // Cập nhật chiều cao của nội dung
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, adjustedContentHeight);
    }

    private void LoadCategoryOnce()
    {
        for (int i = 0; i < itemsController.categoryList.Count; i++)
        {
            CategoryCanvas categoryCanvas = Instantiate(categoryCanvasPrefab, transform).GetComponent<CategoryCanvas>();
            categoryCanvas.gameObject.SetActive(true);
            categoryCanvas.transform.SetParent(transform, false);
            categoryCanvas.LoadData(itemsController.categoryList[i]);
            categoryCanvas.categoryController = this;
            categoryCanvasList.Add(categoryCanvas);
        }
    }

    private void ControlFontSize()
    {
        float minFont = float.MaxValue;
        foreach (TextMeshProUGUI text in textMeshProUGUIsList)
        {
            if (text.fontSize < minFont)
                minFont = text.fontSize;
        }

        foreach (TextMeshProUGUI text in textMeshProUGUIsList)
        {
            text.fontSizeMax = minFont;
        }
    }
}