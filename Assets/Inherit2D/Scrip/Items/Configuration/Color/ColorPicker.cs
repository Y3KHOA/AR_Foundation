using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Lớp này đại diện cho một bộ chọn màu trong trò chơi, cho phép người dùng chọn màu sắc cho các vật phẩm.
/// </summary>
public class ColorPicker : MonoBehaviour
{
    public Image image;
    public GameObject border;
    public int index = 0;
    public float alpha = 1;

    private GameManager gameManager;
    private ColorController colorController;

    private void Start()
    {
        gameManager = GameManager.instance;
        colorController = ColorController.instance;
    }

    public void SelectColorOnClick()
    {
        if (gameManager.itemIndex == null) return;
        ItemCreated itemIndex = gameManager.itemIndex;

        //
        if (colorController.colorPickerIndex != null) colorController.colorPickerIndex.border.SetActive(false);

        //Action
        PreviousAction undoActions = new PreviousAction();
        undoActions.itemId = itemIndex.itemId;
        undoActions.action = "Change Color";
        undoActions.texture = gameManager.itemIndex.sizePointManager.backgroundMeshRenderer.material.GetTexture("_MainTex");
        undoActions.colorPicker = itemIndex.item.colorPicker;
        gameManager.undoActionList.previousActions.Add(undoActions);
        itemIndex.numberOfColorChanges++;

        //
        alpha = 1;
        itemIndex.sizePointManager.ChangeColor(this);
        colorController.colorPickerIndex = this;
        colorController.colorPickerIndex.border.SetActive(true);

        //Slider
        Color newColor = image.sprite.texture.GetPixel(50, 50);
        newColor.a = 1;
        colorController.alphaSlider.value = 1;
        Color startColor = newColor;
        startColor.a = 0;
        colorController.alphaMaterial.SetColor("_GradientStartColor", startColor);
        colorController.alphaMaterial.SetColor("_GradientEndColor", newColor);
    }
}
