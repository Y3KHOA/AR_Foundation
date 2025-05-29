using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý chức năng chọn màu trong trò chơi, cho phép người chơi chọn và sửa đổi màu sắc cho vật phẩm.
/// </summary>
public class ColorController : MonoBehaviour
{
    public static ColorController instance;

    [Header("Prefab")]
    public GameObject colorPrefab;

    [Header("Slider")]
    public Slider alphaSlider;
    public float alphaValue;
    public Material alphaMaterial;
    public TextMeshProUGUI percentText;

    [Header("Panel")]
    public GameObject panelCanvas;
    public RectTransform panelRect;
    public RectTransform colorChartRect;

    [Header("Parent")]
    public GameObject colorChartParent;

    private string folderPath = "ImagesItem/Colors/APP XHERO - Feng Shui & Expert";
    [HideInInspector] public ColorPicker colorPickerIndex;
    private GameManager gameManager;
    private List<ColorPicker> colorPickerList = new List<ColorPicker>();
    private MaterialController materialController;
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rtfColorPickerRect;
    private Camera mainCamera;
    private Vector3 tempPos;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //
        gameManager = GameManager.instance;
        materialController = MaterialController.instance;

        //
        gridLayoutGroup = colorChartParent.GetComponent<GridLayoutGroup>();
        mainCamera = Camera.main;

        //Event
        alphaSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(alphaSlider.value); });

        LoadColorChart();

        panelCanvas.SetActive(false);
    }

    private void FixedUpdate()
    {
        Vector2 touchPosition;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;
        }
        else
        {
            touchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            rtfColorPickerRect = panelCanvas.activeSelf ? panelRect : null;

            if (rtfColorPickerRect != null && !RectTransformUtility.RectangleContainsScreenPoint(rtfColorPickerRect, touchPosition, mainCamera))
            {
                tempPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (rtfColorPickerRect != null && Vector3.Distance(Input.mousePosition, tempPos) < 5f)
            {
                DisableColorPicker();
            }
        }
    }

    private void LoadColorChart()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);
        sprites = sprites.OrderBy(s => ExtractNumber(s.name)).ToArray();

        //Grid
        float height = colorChartRect.rect.height;
        float cellSize = height / sprites.Length;
        gridLayoutGroup.cellSize = new Vector2(cellSize * 11.5f, cellSize * 11.5f);

        for (int i = 0; i < sprites.Length; i++)
        {
            ColorPicker color = Instantiate(colorPrefab, colorChartParent.transform).GetComponent<ColorPicker>();
            color.image.sprite = sprites[i];
            color.index = i;
            colorPickerList.Add(color);
        }
    }

    private int ExtractNumber(string fileName)
    {
        Match match = Regex.Match(fileName, @"\d+"); // Tìm số trong tên file
        return match.Success ? int.Parse(match.Value) : 0;
    }

    private void OnSliderValueChanged(float value)
    {
        alphaValue = value;

        if (colorPickerIndex != null)
        {
            colorPickerIndex.alpha = alphaValue;
            ChangeAlphaColor(alphaValue);
        }
    }

    public void DisableColorPicker()
    {
        panelCanvas.SetActive(false);

        if (gameManager.itemIndex.item.colorPicker == null) return;
        colorPickerIndex = gameManager.itemIndex.item.colorPicker;
        colorPickerIndex.border.SetActive(false);
    }

    public void EnableColorPickerOnClick(MaterialGroundCanvas materialGroundCanvas)
    {
        //Border
        foreach (MaterialGroundCanvas groundCanvas in materialController.materialGroundCanvasList)
        {
            groundCanvas.selectedBorderGO.SetActive(false);
        }
        materialGroundCanvas.selectedBorderGO.SetActive(true);

        //
        if (!panelCanvas.activeSelf)
        {
            panelCanvas.SetActive(true);

            if (gameManager.itemIndex.item.colorPicker == null) return;
            colorPickerIndex = gameManager.itemIndex.item.colorPicker;
            if (gameManager.itemIndex.numberOfColorChanges == 0)
                colorPickerIndex.border.SetActive(false);
            else
                colorPickerIndex.border.SetActive(true);
            alphaSlider.value = colorPickerIndex.alpha;
            Color colorIndex = colorPickerIndex.image.sprite.texture.GetPixel(50, 50);
            Color startColor = colorIndex;
            startColor.a = 0;
            alphaMaterial.SetColor("_GradientStartColor", startColor);
            alphaMaterial.SetColor("_GradientEndColor", colorIndex);
        }
    }

    public void ChangeAlphaColor(float alpha)
    {
        if (gameManager.itemIndex == null) return;
        ItemCreated itemIndex = gameManager.itemIndex;
        itemIndex.sizePointManager.ChangeColor(itemIndex.item.colorPicker);
        alphaSlider.value = alpha;
        UpdatePercentText(alpha);
    }

    public void UpdatePercentText(float alpha)
    {
        percentText.text = ((int)(alpha * 100)).ToString() + "%";
    }
}
