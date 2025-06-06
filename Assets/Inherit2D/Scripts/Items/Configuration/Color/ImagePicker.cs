using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này xử lý biểu ngữ tiêu đề nút trong trò chơi, cho phép người dùng mở cài đặt cấu hình cho vật phẩm hoặc mặt đất.
/// </summary>
public class ImagePicker : MonoBehaviour
{
    private Material groundMaterial;
    private Texture2D groundTexture;
    private Vector2 tilingSize = new Vector2(5f, 5f);

    private GameManager gameManager;
    private MaterialGroundCanvas groundCanvas;
    private MaterialController materialController;
    private const string kindGroundString = "Kết cấu";

    void Start()
    {
        gameManager = GameManager.instance;
        groundCanvas = GetComponent<MaterialGroundCanvas>();
        materialController = MaterialController.instance;
    }

    public void SelectImageOnClick()
    {
        if (gameManager.itemIndex == null || !gameManager.itemIndex.item.CompareKindOfItem(kindGroundString)) return;

        //Action
        PreviousAction undoActions = new PreviousAction();
        undoActions.itemId = gameManager.itemIndex.itemId;
        undoActions.action = "Change Color";
        undoActions.texture = gameManager.itemIndex.sizePointManager.backgroundMeshRenderer.material.GetTexture("_MainTex");
        gameManager.undoActionList.previousActions.Add(undoActions);
        gameManager.itemIndex.numberOfColorChanges++;

        groundMaterial = gameManager.itemIndex.sizePointManager.backgroundMaterialTemp;
        groundTexture = groundCanvas.image.sprite.texture;

        groundMaterial.SetTexture("_MainTex", groundTexture);
        groundMaterial.SetColor("_Color", Color.white);
        groundMaterial.SetVector("_Tiling", new Vector4(tilingSize.x, tilingSize.y, 0, 0));
        gameManager.itemIndex.sizePointManager.backgroundMeshRenderer.material = groundMaterial;
        gameManager.itemIndex.sizePointManager.isUsingImageBackground = true;

        //Border
        foreach (MaterialGroundCanvas groundCanvas in materialController.materialGroundCanvasList)
        {
            groundCanvas.selectedBorderGO.SetActive(false);
        }
        groundCanvas.selectedBorderGO.SetActive(true);
    }
}
