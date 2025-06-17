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

                                // THÊM BƯỚC BACKUP ROOM TRƯỚC KHI XÓA
                                if (ItemRoomMapper.ItemIdToRoomId.TryGetValue(itemIndex.itemId, out string roomId))
                                {
                                    Room room = RoomStorage.rooms.Find(r => r.ID == roomId);
                                    if (room != null)
                                    {
                                        pAction.roomBackup = new Room(room); // 👈 Backup lại vào pAction
                                        RoomStorage.rooms.Remove(room);
                                        Debug.Log($"[Undo] Xoá Room ID = {roomId}");
                                    }

                                    ItemRoomMapper.ItemIdToRoomId.Remove(itemIndex.itemId);
                                }

                                // Redo
                                ItemCreated itemRedo = Instantiate(itemIndex, gameManager.redoItemsParent.transform);
                                itemRedo.gameObject.SetActive(false);
                                gameManager.redoItemsList.Add(itemRedo);

                                // Undo
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

                            // Phục hồi Room từ action.roomBackup
                            if (pAction.roomBackup != null)
                            {
                                Room restoredRoom = new Room(pAction.roomBackup); // Clone lại nếu cần tránh reference cũ
                                RoomStorage.rooms.Add(restoredRoom);
                                ItemRoomMapper.ItemIdToRoomId[item.itemId] = restoredRoom.ID;
                                item.sizePointManager.SetCurrentRoom(restoredRoom); // gán lại Room cho SizePointManager
                                Debug.Log($"[Undo] Khôi phục Room ID = {restoredRoom.ID}");
                            }
                            else
                            {
                                Debug.LogWarning("[Undo] Không có roomBackup trong PreviousAction để khôi phục!");
                            }

                            gameManager.deleteItems2DList.RemoveAt(indexD);
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

                                // Cập nhật lại Room
                                item.sizePointManager.UpdateRoomDataFromSizePoints();
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

                                // Cập nhật lại Room
                                item.sizePointManager.UpdateRoomDataFromSizePoints();
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
                            Debug.Log("hello i'm here!");
                            int itemId = pAction.itemId;

                            // Tìm trong redoItemsList
                            ItemCreated itemBackup = gameManager.redoItemsList.Find(x => x.itemId == itemId);
                            if (itemBackup == null)
                            {
                                Debug.LogWarning($"[Redo-Create] Không tìm thấy itemId = {itemId} trong redoItemsList");
                                break;
                            }

                            gameManager.redoItemsList.Remove(itemBackup);

                            // Clone lại item
                            gameManager.numberOfItemsCreated++;
                            ItemCreated item = Instantiate(itemBackup, gameManager.createdItems2DParent.transform);
                            item.hasItem = false;
                            item.gameObject.SetActive(true);
                            item.sizePointManager.EnableSizePoint(false);
                            item.transform.position = pAction.position;
                            item.transform.rotation = pAction.rotation;
                            gameManager.createdItems2DList.Add(item);

                            // Gán lại Room nếu có backup
                            if (pAction.roomBackup != null)
                            {
                                Room restoredRoom = new Room(pAction.roomBackup);
                                if (restoredRoom != null)
                                {
                                    RoomStorage.rooms.Add(restoredRoom);
                                    item.itemId = itemId;

                                    ItemRoomMapper.ItemIdToRoomId[item.itemId] = restoredRoom.ID;

                                    if (item.sizePointManager != null)
                                    {
                                        item.sizePointManager.SetCurrentRoom(restoredRoom);
                                        item.sizePointManager.UpdateRoomDataFromSizePoints();
                                    }
                                    else
                                    {
                                        Debug.LogError($"[Redo] item.sizePointManager null với itemId = {item.itemId}");
                                    }

                                    Debug.Log($"[Redo] Khôi phục Room ID = {restoredRoom.ID}");
                                }
                                else
                                {
                                    Debug.LogError("[Redo] Lỗi khi clone Room từ roomBackup");
                                }
                            }
                            else
                            {
                                Debug.LogError("[Redo] roomBackup null");
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

                                // Xoá Room nếu có ánh xạ
                                if (ItemRoomMapper.ItemIdToRoomId.TryGetValue(itemId, out string roomId))
                                {
                                    Room room = RoomStorage.rooms.Find(r => r.ID == roomId);
                                    if (room != null)
                                    {
                                        RoomStorage.rooms.Remove(room);
                                        Debug.Log($"[Redo] Xoá Room ID = {roomId}");
                                    }

                                    ItemRoomMapper.ItemIdToRoomId.Remove(itemId);
                                }
        
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
                                
                                // Cập nhật lại Room
                                item.sizePointManager.UpdateRoomDataFromSizePoints();
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
                                
                                // Cập nhật lại Room
                                item.sizePointManager.UpdateRoomDataFromSizePoints();
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
                // 1. Xóa khỏi danh sách item
                gameManager.createdItems2DList.Remove(item);

                // 2. Lưu bản sao bị xóa
                if (item != null && item.gameObject != null)
                {
                    // Tạo bản sao và ẩn đi
                    ItemCreated itemD = Instantiate(item, gameManager.deleteItems2DParent.transform);
                    itemD.gameObject.SetActive(false);
                    gameManager.deleteItems2DList.Add(itemD);
                }

                // 3. Lưu vào Undo
                PreviousAction action = new PreviousAction();
                action.itemId = item.itemId;
                action.action = "Delete";
                action.position = item.transform.position;
                action.rotation = item.transform.rotation;
                
                // Bắt buộc cập nhật dữ liệu Room trước khi sao chép
                item.sizePointManager.UpdateRoomDataFromSizePoints();

                Room currentRoom = item.sizePointManager.CurrentRoom;
                if (currentRoom != null)
                {
                    action.roomBackup = new Room(currentRoom); // Clone snapshot
                }
                else
                {
                    Debug.LogWarning("[BackupRoom] CurrentRoom null khi xóa");
                }

                gameManager.undoActionList.previousActions.Add(action);


                // 4. Xoá Room trong RoomStorage (theo ánh xạ ID)
                if (ItemRoomMapper.ItemIdToRoomId.TryGetValue(item.itemId, out string roomId))
                {
                    Room room = RoomStorage.rooms.Find(r => r.ID == roomId);
                    if (room != null)
                    {
                        RoomStorage.rooms.Remove(room);
                    }

                    ItemRoomMapper.ItemIdToRoomId.Remove(item.itemId);

                    Debug.Log($"[Delete] Đã xóa Room ID = {roomId} từ storage và file.");
                }
                if (!ItemRoomMapper.ItemIdToRoomId.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"[Delete] Không tìm thấy ánh xạ itemId = {item.itemId} trong ItemRoomMapper!");
            }
                // 5. Xoá item trên scene
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
