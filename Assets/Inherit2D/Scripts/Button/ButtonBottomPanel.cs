using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý các nút bảng điều khiển phía dưới trong trò chơi, bao gồm các chức năng hoàn tác, làm lại, xóa, khóa và danh mục.
/// </summary>
public class ButtonBottomPanel : MonoBehaviour
{
    public ButtonCatalogPanel buttonCatalogPanel;
    public GameObject view360BTN;

    [Header("Lock Button")]
    public Button lockBTN;
    public Sprite lockSprite;
    public Sprite unLockSprite;

    private GameManager gameManager;
    private Configuration config;
    private GridLayoutGroup gridLayoutGroup;
    private List<Button> buttonsList;
    private RectTransform rectTransform;
    private const string kindGroundString = "Kết cấu";

    private void Start()
    {
        gameManager = GameManager.instance;
        config = Configuration.instance;

        //view360BTN.SetActive(false);
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        buttonsList = GetComponentsInChildren<Button>().ToList();
        rectTransform = GetComponent<RectTransform>();
        ControlButtonsSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        ControlButtonsSize();
    }

    private void ControlButtonsSize()
    {
        if (rectTransform == null && buttonsList == null) return;

        int numberOfButtonActive = 0;
        foreach (Button button in buttonsList)
        {
            if (button.gameObject.activeSelf)
                numberOfButtonActive++;
        }

        if (numberOfButtonActive <= 0) return;

        float cellSize = Screen.width / numberOfButtonActive;
        cellSize -= numberOfButtonActive;
        cellSize = Mathf.Clamp(cellSize, 10, rectTransform.rect.height);

        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        gridLayoutGroup.spacing = new Vector2(numberOfButtonActive * 0.75f, 0);
    }

    public void OpenCatalogOnClick()
    {
        int t = 0;
        if (gameManager.guiCanvasManager.catalogBannerRect.localScale.x == 0) t = 1;
        else t = 0;

        gameManager.guiCanvasManager.catalogBannerRect.localScale = new Vector3(t, 1, 1);
        gameManager.guiCanvasManager.categoryCanvas.SetActive(false);
        buttonCatalogPanel.openCatalogButton.image.sprite = buttonCatalogPanel.normalSprite;
        gameManager.itemHasChosen.gameObject.SetActive(false);
    }

    public void UndoOnClick()
    {
        if (gameManager.itemHasChosen.gameObject.activeSelf)
        {
            gameManager.itemHasChosen.CancelChosenItem();
            return;
        }

        if (gameManager.undoActionList.previousActions.Count > 0)
        {
            //Lấy action trong danh sách action
            int indexAction = gameManager.undoActionList.previousActions.Count - 1;
            PreviousAction pAction = gameManager.undoActionList.previousActions[indexAction];

            if (pAction != null)
            {
                switch (pAction.action)
                {
                    case "Create":
                        {
                            //Item created
                            if (gameManager.createdItems2DList.Count > 0)
                            {
                                int index = gameManager.createdItems2DList.Count - 1;
                                ItemCreated itemIndex = gameManager.createdItems2DList[index];

                                //Redo
                                ItemCreated itemRedo = Instantiate(itemIndex, gameManager.redoItemsParent.transform);
                                itemRedo.gameObject.SetActive(false);
                                gameManager.redoItemsList.Add(itemRedo);

                                //Undo
                                Destroy(itemIndex.gameObject);
                                gameManager.createdItems2DList.RemoveAt(index);
                            }
                            break;
                        }
                    case "Delete":
                        {
                            //Lấy phần tử đã xóa trước đó trước đó
                            int indexD = gameManager.deleteItems2DList.Count - 1;
                            ItemCreated itemD = gameManager.deleteItems2DList[indexD];

                            //Redo
                            ItemCreated itemRedo = Instantiate(itemD, gameManager.redoItemsParent.transform);
                            itemRedo.gameObject.SetActive(false);
                            gameManager.redoItemsList.Add(itemRedo);

                            //Undo
                            gameManager.numberOfItemsCreated++;
                            ItemCreated item = Instantiate(itemD, gameManager.createdItems2DParent.transform);
                            item.hasItem = false;
                            item.gameObject.SetActive(true);
                            item.sizePointManager.EnableSizePoint(false);
                            item.transform.position = pAction.position;
                            item.transform.rotation = pAction.rotation;

                            gameManager.createdItems2DList.Add(item);
                            gameManager.deleteItems2DList.RemoveAt(indexD);
                            //Destroy(item.item3DIndex.gameObject);
                            Destroy(itemD.gameObject);
                            break;
                        }
                    case "Move":
                        {
                            ItemCreated item = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);
                            if (item != null)
                            {
                                Vector3 pos = item.transform.position;
                                Vector3 rot = item.transform.rotation.eulerAngles;
                                item.transform.position = pAction.position;
                                Vector3 targetRotationEuler = pAction.rotation.eulerAngles;
                                item.transform.rotation = Quaternion.Euler(0, 0, targetRotationEuler.z);

                                pAction.position = pos;
                                pAction.rotation.eulerAngles = rot;
                            }
                            break;
                        }
                    case "Change Color":
                        {
                            ItemCreated itemIndex = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);
                            itemIndex.numberOfColorChanges--;

                            //Đổi màu
                            if (itemIndex != null)
                            {
                                if (pAction.texture != null)
                                {
                                    Texture texture = itemIndex.sizePointManager.backgroundMaterialTemp.GetTexture("_MainTex");
                                    Material groundMaterial;
                                    if (itemIndex.numberOfColorChanges == 0)
                                    {
                                        itemIndex.sizePointManager.SetDefaultColor();
                                    }
                                    else
                                    {
                                        groundMaterial = itemIndex.sizePointManager.backgroundMaterialTemp;
                                        groundMaterial.SetTexture("_MainTex", pAction.texture);
                                        groundMaterial.SetColor("_Color", Color.white);
                                        groundMaterial.SetVector("_Tiling", new Vector4(5, 5, 0, 0));
                                        itemIndex.sizePointManager.backgroundMeshRenderer.material = groundMaterial;
                                    }
                                    pAction.texture = texture;
                                }
                                else
                                {
                                    ColorPicker colorPicker = itemIndex.item.colorPicker;
                                    if (pAction.colorPicker != null)
                                    {
                                        itemIndex.item.colorPicker = pAction.colorPicker;
                                        itemIndex.sizePointManager.ChangeColor(itemIndex.item.colorPicker);
                                    }
                                    if (itemIndex.numberOfColorChanges == 0)
                                    {
                                        itemIndex.sizePointManager.SetDefaultColor();
                                    }

                                    pAction.colorPicker = colorPicker;
                                }
                            }
                            break;
                        }
                    case "Change Size":
                        {
                            ItemCreated item = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);

                            List<Vector3> sizePointPosCurrentList = new List<Vector3>();
                            for (int i = 0; i < item.sizePointManager.sizePointList.Count; i++)
                            {
                                sizePointPosCurrentList.Add(Vector3.zero);
                                sizePointPosCurrentList[i] = item.sizePointManager.sizePointList[i].transform.localPosition;
                            }

                            if (item != null)
                            {
                                for (int i = 0; i < item.sizePointManager.sizePointList.Count; i++)
                                {
                                    item.sizePointManager.sizePointList[i].transform.localPosition = pAction.sizePointPosList[i];
                                }

                                item.sizePointManager.UpdateLineRenderer();
                            }

                            //config.UpdateInfomationItem();
                            pAction.sizePointPosList = sizePointPosCurrentList;
                            break;
                        }
                }

                gameManager.undoActionList.previousActions.RemoveAt(indexAction);
                gameManager.redoActionList.previousActions.Add(pAction);
            }
        }
    }

    public void RedoOnClick()
    {
        if (gameManager.itemHasChosen.gameObject.activeSelf)
        {
            gameManager.itemHasChosen.CancelChosenItem();
            return;
        }

        if (gameManager.redoActionList.previousActions.Count > 0)
        {
            //Lấy action trong danh sách action
            int indexAction = gameManager.redoActionList.previousActions.Count - 1;
            PreviousAction pAction = gameManager.redoActionList.previousActions[indexAction];
            if (pAction != null)
            {
                switch (pAction.action)
                {
                    case "Create":
                        {
                            //Item created
                            if (gameManager.redoItemsList.Count > 0)
                            {
                                int itemId = pAction.itemId;
                                ItemCreated itemIndex = gameManager.redoItemsList.Find(x => x.itemId == itemId);
                                itemIndex.gameObject.SetActive(true);
                                itemIndex.sizePointManager.item = itemIndex.item;
                                itemIndex.sizePointManager.EnableSizePoint(false);
                                if (itemIndex.item.CompareKindOfItem(kindGroundString))
                                {
                                    pAction.position.z = -0.5f;
                                }
                                itemIndex.transform.position = pAction.position;
                                itemIndex.transform.SetParent(gameManager.createdItems2DParent.transform);
                                gameManager.createdItems2DList.Add(itemIndex);
                                gameManager.redoItemsList.Remove(gameManager.redoItemsList.Find(x => x.itemId == itemId));
                            }
                            break;
                        }
                    case "Delete":
                        {
                            if (gameManager.redoItemsList.Count > 0)
                            {
                                //Lấy phần tử đã xóa trước đó trước đó
                                int itemId = pAction.itemId;

                                ItemCreated itemIndex = gameManager.redoItemsList.Find(x => x.itemId == itemId);
                                gameManager.redoItemsList.Remove(itemIndex);
                                gameManager.deleteItems2DList.Add(itemIndex);
                                itemIndex.transform.SetParent(gameManager.deleteItems2DParent.transform);
                                itemIndex.gameObject.SetActive(false);

                                ItemCreated itemCreated = gameManager.createdItems2DList.Find(x => x.itemId == itemId);
                                gameManager.createdItems2DList.Remove(itemCreated);
                                Destroy(itemCreated.gameObject);
                            }
                            break;
                        }
                    case "Move":
                        {
                            ItemCreated item = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);
                            if (item != null)
                            {
                                Vector3 pos = item.transform.position;
                                Vector3 rot = item.transform.rotation.eulerAngles;

                                item.transform.position = pAction.position;
                                Vector3 targetRotationEuler = pAction.rotation.eulerAngles;
                                item.transform.rotation = Quaternion.Euler(0, 0, targetRotationEuler.z);

                                pAction.position = pos;
                                pAction.rotation.eulerAngles = rot;
                            }
                            break;
                        }
                    case "Change Color":
                        {
                            ItemCreated itemIndex = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);
                            itemIndex.numberOfColorChanges++;

                            //Đổi màu
                            if (itemIndex != null)
                            {
                                if (pAction.texture != null)
                                {
                                    Texture texture = itemIndex.sizePointManager.backgroundMaterialTemp.GetTexture("_MainTex");
                                    Material groundMaterial;
                                    if (itemIndex.numberOfColorChanges == 0)
                                    {
                                        itemIndex.sizePointManager.SetDefaultColor();
                                    }
                                    else
                                    {
                                        groundMaterial = itemIndex.sizePointManager.backgroundMaterialTemp;

                                        groundMaterial.SetTexture("_MainTex", pAction.texture);
                                        groundMaterial.SetColor("_Color", Color.white);
                                        groundMaterial.SetVector("_Tiling", new Vector4(5, 5, 0, 0));
                                        itemIndex.sizePointManager.backgroundMeshRenderer.material = groundMaterial;
                                    }
                                    pAction.texture = texture;
                                }
                                else
                                {
                                    ColorPicker colorPicker = itemIndex.item.colorPicker;
                                    if (pAction.colorPicker != null)
                                    {
                                        itemIndex.item.colorPicker = pAction.colorPicker;
                                        itemIndex.sizePointManager.ChangeColor(itemIndex.item.colorPicker);
                                    }
                                    else
                                    {
                                        itemIndex.sizePointManager.SetDefaultColor();
                                    }

                                    pAction.colorPicker = colorPicker;
                                }
                            }
                            break;
                        }
                    case "Change Size":
                        {
                            ItemCreated item = gameManager.createdItems2DList.Find(x => x.itemId == pAction.itemId);

                            List<Vector3> sizePointPosCurrentList = new List<Vector3>();
                            for (int i = 0; i < item.sizePointManager.sizePointList.Count; i++)
                            {
                                sizePointPosCurrentList.Add(Vector3.zero);
                                sizePointPosCurrentList[i] = item.sizePointManager.sizePointList[i].transform.localPosition;
                            }

                            if (item != null)
                            {
                                for (int i = 0; i < item.sizePointManager.sizePointList.Count; i++)
                                {
                                    item.sizePointManager.sizePointList[i].transform.localPosition = pAction.sizePointPosList[i];
                                }

                                item.sizePointManager.UpdateLineRenderer();
                            }

                            //config.UpdateInfomationItem();
                            pAction.sizePointPosList = sizePointPosCurrentList;
                            break;
                        }
                }

                gameManager.redoActionList.previousActions.RemoveAt(indexAction);
                gameManager.undoActionList.previousActions.Add(pAction);
            }
        }
    }

    public void DeleteOnClick()
    {
        if (gameManager.itemIndex != null)
        {
            ItemCreated item = gameManager.createdItems2DList.Find(x => x.itemId == gameManager.itemIndex.itemId);
            if (item != null)
            {
                //Xóa khỏi danh sách item
                gameManager.createdItems2DList.Remove(item);

                //Thêm vào danh sách item đã xóa
                ItemCreated itemD = Instantiate(item, gameManager.deleteItems2DParent.transform);
                itemD.gameObject.SetActive(false);
                gameManager.deleteItems2DList.Add(itemD);

                //Thêm vào lịch sử thao tác
                PreviousAction action = new PreviousAction();
                action.itemId = item.itemId;
                action.action = "Delete";
                action.position = item.transform.position;
                action.rotation = item.transform.rotation;
                gameManager.undoActionList.previousActions.Add(action);

                //Xóa gameobject
                Destroy(item.gameObject);
            }
            gameManager.itemIndex = null;
        }
    }

    public void LockOnClick()
    {
        if (gameManager.isLock)
        {
            gameManager.isLock = false;
            lockBTN.image.sprite = unLockSprite;
        }
        else
        {
            gameManager.isLock = true;
            lockBTN.image.sprite = lockSprite;
        }
    }
}
