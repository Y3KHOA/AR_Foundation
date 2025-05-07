using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using TMPro;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public class ItemsController : MonoBehaviour
{
    public static ItemsController instance;
    private GameManager gameManager;
    private LoadData loadData;
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;
    private ScrollRect scrollRect;

    [Header("Item")]
    //public GameObject itemPrefab;
    public List<Item> itemsList = new List<Item>();
    private List<ItemCanvas> itemCanvasList = new List<ItemCanvas>();
    public List<Category> categoryList = new List<Category>();
    private float cellSize;
    private bool isControlSizeOnceTime = false;
    private bool isFiltering = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameManager.instance;
        loadData = LoadData.instance;

        //Category
        categoryList = loadData.LoadCategories();

        //Item
        itemsList = loadData.LoadItems();
        LoadItemsOnce();
        gameManager.itemHasChosen.gameObject.SetActive(false);

        //Adjust
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponentInParent<ScrollRect>();

        ControllItemsBoxCellSize();
        if (rectTransform != null)
        {
            UpdateBottom();

            if (!isControlSizeOnceTime)
            {
                //Điều chỉnh top
                Vector2 offsetMax = rectTransform.offsetMax;
                offsetMax.y = -offsetMax.y * 4;
                rectTransform.offsetMax = offsetMax;
                isControlSizeOnceTime = true;
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isFiltering) return;

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
        cellSize = witdh;
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize * 1.15f);

        //Font
        ControlFontSize();
    }

    private void UpdateBottom()
    {
        if (gridLayoutGroup == null || itemsList.Count == 0) return;

        float contentHeight = gridLayoutGroup.cellSize.y * Mathf.Ceil(itemsList.Count / (float)gridLayoutGroup.constraintCount);

        // Đảm bảo rằng chiều cao này không nhỏ hơn chiều cao của viewport
        float viewportHeight = scrollRect.viewport.rect.height;
        float adjustedContentHeight = Mathf.Max(contentHeight, viewportHeight);
        adjustedContentHeight -= viewportHeight;

        // Cập nhật chiều cao của nội dung
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, adjustedContentHeight);
    }

    private void UpdateBottom(int numberOfItem)
    {
        if (gridLayoutGroup == null || numberOfItem == 0) return;

        float contentHeight = gridLayoutGroup.cellSize.y * Mathf.Ceil(numberOfItem / (float)gridLayoutGroup.constraintCount);

        // Đảm bảo rằng chiều cao này không nhỏ hơn chiều cao của viewport
        float viewportHeight = scrollRect.viewport.rect.height;
        float adjustedContentHeight = Mathf.Max(contentHeight, viewportHeight);
        adjustedContentHeight -= viewportHeight;

        // Cập nhật chiều cao của nội dung
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, adjustedContentHeight);
    }


    //Tải 1 lần duy nhất khi chạy
    private void LoadItemsOnce()
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            ItemCanvas itemCanvas = gameManager.itemCanvasPool.GetObjectFromPool().GetComponent<ItemCanvas>();
            itemCanvas.gameObject.SetActive(true);
            itemCanvas.transform.SetParent(transform, false);
            itemCanvas.item = itemsList[i];
            itemCanvas.LoadData();
            itemCanvasList.Add(itemCanvas);

            if(!itemCanvas.item.CompareKindOfItem("Thao tác"))
            {
                foreach (string kind in itemCanvas.item.kindsOfItem)
                {
                    categoryList.FirstOrDefault(x => x.categoryName == kind).CountNumberOfItem();
                }
            }    
        }
    }  
    
    private void ControlFontSize()
    {
        float minFont = float.MaxValue;
        foreach (ItemCanvas item in itemCanvasList)
        {
            if (item.itemNameText.fontSize < minFont)
                minFont = item.itemNameText.fontSize;
        }

        foreach (ItemCanvas item in itemCanvasList)
        {
            item.itemNameText.fontSizeMax = minFont;
        }
    }

    public void FilterItem(string filter = "")
    {
        isFiltering = true;
        int numberOfItem = 0;

        itemCanvasList.ForEach(
            x =>
            {
                if (!x.item.CompareKindOfItem(filter) && filter != "")
                {
                    x.gameObject.SetActive(false);
                }
                else
                {
                    x.gameObject.SetActive(true);
                    numberOfItem++;
                }
            });

        UpdateBottom(numberOfItem);

        // Đặt lại vị trí cuộn về đầu
        scrollRect.verticalNormalizedPosition = 1.0f;

        isFiltering = false;
    }

}
