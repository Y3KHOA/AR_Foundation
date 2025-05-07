using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryCanvas : MonoBehaviour
{
    [Header("Border")]
    public GameObject selectedBorder;
    public GameObject unselectedBorder;

    [Header("Icon")]
    public Image icon;

    [Header("Text")]
    public TextMeshProUGUI textMeshProUGUI;

    [HideInInspector] public CategoryController categoryController;
    private ItemsController itemsController;
    private Category categoryTemp;

    private void Start()
    {
        itemsController = ItemsController.instance;
    }

    public void LoadData(Category category)
    {
        categoryTemp = category;

        Sprite sprite = Resources.Load<Sprite>($"ImagesItem/{category.categoryName}");
        if (sprite != null)
        {
            icon.sprite = sprite;
        }
        else
        {
            Debug.LogError($"Failed to load image '{category.categoryName}'");
            icon.sprite = null;
        }

        textMeshProUGUI.text = category.categoryName + " (" + category.numberOfItem + ")";
    }    

    public void LoadItemOnClick()
    {
        itemsController.FilterItem(categoryTemp.categoryName);

        unselectedBorder.gameObject.SetActive(false);
        foreach (CategoryCanvas categoryCanvas in categoryController.categoryCanvasList)
        {
            if(categoryCanvas != this)
            {
                categoryCanvas.unselectedBorder.SetActive(true);

            }
        }
    }
}
