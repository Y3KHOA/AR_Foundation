using UnityEngine;
using UnityEngine.UI;

public class ButtonCatalogPanel : MonoBehaviour
{
    [Header("Open Catalog Button")]
    public Button openCatalogButton;
    public Sprite normalSprite;
    public Sprite closeSprite;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void OpenCategoryOnClick()
    {
        if(gameManager.guiCanvasManager.categoryCanvas.activeSelf == true)
        {
            openCatalogButton.image.sprite = normalSprite;
        }
        else
        {
            openCatalogButton.image.sprite = closeSprite;
        }
        gameManager.guiCanvasManager.categoryCanvas.SetActive(!gameManager.guiCanvasManager.categoryCanvas.activeSelf);
    }
}
