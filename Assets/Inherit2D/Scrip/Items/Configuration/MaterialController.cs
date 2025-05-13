using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialController : MonoBehaviour
{
    public static MaterialController instance;

    [Header("Prefab")]
    public GameObject materialPrefab;

    private List<MaterialGround> materialsList = new List<MaterialGround>();
    public List<MaterialGroundCanvas> materialGroundCanvasList = new List<MaterialGroundCanvas>();
    private LoadData loadData;
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;
    private float cellSize;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        loadData= LoadData.instance;
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        LoadMaterialsOnce();
        ControllMaterialBoxCellSize();

        transform.parent.gameObject.SetActive(false);
    }

    private void LoadMaterialsOnce()
    {
        materialsList = loadData.LoadMaterialsGround();

        for (int i = 0; i < materialsList.Count; i++)
        {
            MaterialGroundCanvas materialGroundCanvas = Instantiate(materialPrefab, gameObject.transform).GetComponent<MaterialGroundCanvas>();
            materialGroundCanvas.materialGround = materialsList[i];
            materialGroundCanvas.LoadData();
            materialGroundCanvasList.Add(materialGroundCanvas);
        }
    }

    private void ControllMaterialBoxCellSize()
    {
        if (gridLayoutGroup == null) return;
        float witdh = rectTransform.rect.width;
        cellSize = witdh / 2.2f;
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        gridLayoutGroup.spacing = new Vector2(cellSize / 13f, cellSize / 13f);
    }

    public void ControlFontSize()
    {
        float minFont = float.MaxValue;
        foreach (MaterialGroundCanvas materialCanvas in materialGroundCanvasList)
        {
            if (materialCanvas.nameText.fontSize < minFont)
                minFont = materialCanvas.nameText.fontSize;
        }

        foreach (MaterialGroundCanvas materialCanvas in materialGroundCanvasList)
        {
            materialCanvas.nameText.fontSizeMax = minFont;
        }
    }

    public IEnumerator DelayedControlFontSize()
    {
        yield return new WaitForEndOfFrame(); // Chờ 1 frame để layout cập nhật
        ControlFontSize();
    }
}
