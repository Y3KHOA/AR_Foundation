using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý việc tạo và điều khiển các mục trong không gian làm việc của trò chơi.
/// </summary>
public class ItemCreated : MonoBehaviour
{
    [Header("Item")]
    public Item item;
    public int itemId = 0;
    public bool hasItem = false;

    [Header("Rotation")]
    public RotationButton rotationBTN;
    public bool isRotate = false;

    [Header("Move")]
    public MoveButton moveBTN;
    [HideInInspector] public bool isMoving = false;

    [Header("Size")]
    public SizePointManager sizePointManager;

    [Header("Room")]
    public GameObject roomParent;
    public ItemCreated roomList;

    [Header("Setting")]
    public RectTransform rectTransform;
    public int numberOfColorChanges = 0;

    //Components
    private const string kindGroundString = "Kết cấu";
    private const string kindWallHangings = "Treo tường";
    private GameManager gameManager;
    private RectTransform board2dRect;
    private string direction = "";
    private PointerEventData pointerData = new PointerEventData(EventSystem.current);
    private Vector3 clickOffset; // Lưu khoảng cách giữa tâm đối tượng và điểm nhấp chuột
    private Vector3 mousePosition;
    private Vector3 tempMouse;
    private bool mouseMove = false;
    private List<RaycastResult> results = new List<RaycastResult>();

    public Item tempItem; // Dữ liệu tạm để chỉnh


    private void Start()
    {
        gameManager = GameManager.instance;

        //Rotation
        rotationBTN.Start();
        rotationBTN.gameObject.SetActive(false);

        //Move
        moveBTN.Start();
        moveBTN.gameObject.SetActive(false);

        //Size
        sizePointManager = GetComponentInChildren<SizePointManager>();
        sizePointManager.UpdateAreaText();

        //
        board2dRect = gameManager.guiCanvasManager.board2dRect;
    }

    private void Update()
    {
        if (gameManager.isLock)
        {
            if (gameManager.itemIndex != null)
            {
                gameManager.itemIndex.sizePointManager.EnableSizePoint(false);
                gameManager.itemIndex.rotationBTN.gameObject.SetActive(false);
                gameManager.itemIndex.moveBTN.gameObject.SetActive(false);
            }
            return;
        }

        MouseButtonDown();

        MouseButton();

        MouseButtonUp();
    }

    private void MouseButtonDown()
    {
        if ((Input.GetMouseButtonDown(0) && gameManager.guiCanvasManager.isOnWordSpace || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && gameManager.guiCanvasManager.isOnWordSpace)
        {
            tempMouse = Input.mousePosition;

            if (gameManager.itemIndex != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                pointerData.position = Input.mousePosition;
                results.Clear();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("Test2"))
                    {
                        gameManager.sizePointClick = true;
                    }
                }

                if (isMoving)
                {
                    SetMousePosition(board2dRect);
                    clickOffset = gameManager.itemIndex.transform.position - mousePosition;
                }

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("UI")))
                {
                    if (hit.collider.CompareTag("Test"))
                    {
                        ItemCreated it = hit.collider.GetComponentInParent<ItemCreated>();

                        if (it != null)
                        {
                            if (it == gameManager.itemIndex || gameManager.itemIndex.isMoving)
                            {
                                gameManager.itemIndex.hasItem = true;
                            }
                            else
                            {
                                gameManager.itemIndex.hasItem = false;
                            }
                            it.clickOffset = it.transform.position - hit.point;
                        }
                    }
                }
                else
                {
                    if (!gameManager.sizePointClick && gameManager.itemIndex != null)
                    {
                        gameManager.itemIndex.hasItem = false;
                    }
                }
            }
        }
    }

    private void MouseButton()
    {
        if (Input.GetMouseButton(0))
        {
            if (tempMouse != Input.mousePosition)
            {
                mouseMove = true;
            }

            if (gameManager.itemIndex != null)
            {
                if (hasItem && gameManager.guiCanvasManager.isOnWordSpace)
                {
                    if (!gameManager.sizePointClick || isMoving)
                    {
                        if (gameManager.itemIndex.item.CompareKindOfItem(kindWallHangings))
                        {
                            SetPositionWallHangings(gameManager.itemIndex);
                        }
                        else
                        {
                            if (!gameManager.GetDrawingStatus()) //Nếu đang thêm tường
                            {
                                SetMousePosition(board2dRect);
                                SetPosition();
                            }
                        }
                        gameManager.hasItem = true;

                        if (gameManager.itemCreatedOldPosition == Vector3.zero && gameManager.itemCreatedOldRotation == Vector3.zero)
                        {
                            gameManager.itemCreatedOldPosition = transform.position;
                            gameManager.itemCreatedOldRotation = transform.rotation.eulerAngles;
                        }

                        // Cập nhật Room tại đây
                        sizePointManager.UpdateRoomDataFromSizePoints();
                    }
                }
            }
        }
    }

    private void MouseButtonUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!mouseMove)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("UI")))
                {
                    if (hit.collider.CompareTag("Test") && !gameManager.sizePointClick)
                    {
                        ItemCreated it = hit.collider.GetComponentInParent<ItemCreated>();
                        if (it != null)
                        {
                            if (gameManager.itemIndex != null)
                            {
                                gameManager.itemIndex.sizePointManager.EnableSizePoint(false);
                                gameManager.itemIndex.rotationBTN.gameObject.SetActive(false);
                                gameManager.itemIndex.moveBTN.gameObject.SetActive(false);
                            }

                            gameManager.itemIndex = it;
                            it.hasItem = true;

                            //Size point
                            it.sizePointManager.EnableSizePoint(true);

                            //Offset
                            it.clickOffset = it.transform.position - hit.point;

                            //Infomation Item
                            gameManager.guiCanvasManager.infomationItemCanvas.gameObject.SetActive(true);
                            gameManager.guiCanvasManager.infomationItemCanvas.UpdateInfomation(it.item);
                            // Item showItem = it.tempItem != null ? it.tempItem : it.item;
                            // gameManager.guiCanvasManager.infomationItemCanvas.UpdateInfomation(showItem);

                            //Rotation and Move
                            if (gameManager.itemIndex.item.CompareKindOfItem("Thao tác"))
                            {
                                gameManager.itemIndex.rotationBTN.gameObject.SetActive(false);
                                gameManager.itemIndex.moveBTN.gameObject.SetActive(false);
                            }
                            else
                            {
                                gameManager.itemIndex.rotationBTN.gameObject.SetActive(true);
                                gameManager.itemIndex.moveBTN.gameObject.SetActive(true);
                            }
                            gameManager.itemIndex.rotationBTN.UpdatePosition();
                            gameManager.itemIndex.rotationBTN.AdjustSizePointToCamera();
                            gameManager.itemIndex.moveBTN.UpdatePosition();
                            gameManager.itemIndex.moveBTN.AdjustSizePointToCamera();
                        }
                    }
                }
                else
                {
                    ClearSelection();
                }
            }

            mouseMove = false;
            gameManager.hasItem = false;
            gameManager.sizePointClick = false;
            SavePreviousMoveAction();
        }
    }

    private void ClearSelection()
    {
        if (!gameManager.sizePointClick)
        {
            if (gameManager.itemIndex != null)
            {
                gameManager.itemIndex.sizePointManager.EnableSizePoint(false);
                gameManager.itemIndex.rotationBTN.gameObject.SetActive(false);
                gameManager.itemIndex.moveBTN.gameObject.SetActive(false);
                gameManager.itemIndex.hasItem = false;
                gameManager.itemIndex = null;
            }

            if (gameManager.guiCanvasManager.infomationItemCanvas.gameObject.activeSelf)
            {
                gameManager.guiCanvasManager.infomationItemCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void SetMousePosition(RectTransform boardRect)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            boardRect,
            Input.mousePosition,
            Camera.main,
            out mousePosition
        );
    }

    private void SetPosition()
    {
        Vector3 targetPosition = mousePosition + clickOffset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, -board2dRect.rect.width / 2, board2dRect.rect.width / 2);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -board2dRect.rect.height / 2, board2dRect.rect.height / 2);
        if (gameManager.itemIndex.item.CompareKindOfItem(kindGroundString))
        {
            targetPosition.z = -0.5f;
        }
        else
        {
            targetPosition.z = -1.5f;
        }
        //if (gameManager.itemIndex.item.CompareKindOfItem("Thao tác"))
        //{
        //    targetPosition.z = -1f;
        //}    
        gameManager.itemIndex.transform.position = targetPosition;
    }

    private void SetPositionWallHangings(ItemCreated itemCreated)
    {
        if (!isMoving) return; // Chỉ di chuyển nếu đang ở trạng thái di chuyển

        if (gameManager.createdGroudList.Count == 0) return;

        SetMousePosition(board2dRect);
        Vector3 mouseWorldPosition = mousePosition + clickOffset;

        Collider[] nearbyColliders = Physics.OverlapSphere(mouseWorldPosition, Camera.main.orthographicSize);

        Collider nearestCollider = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.CompareTag("Wall"))
            {
                float distance = Vector3.Distance(mouseWorldPosition, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCollider = collider;
                }
            }
        }

        if (nearestCollider != null)
        {
            Vector3 pos = nearestCollider.ClosestPoint(mouseWorldPosition);
            pos.z = -1.5f;
            itemCreated.transform.position = pos;
        }
    }


    private void SavePreviousMoveAction()
    {
        if (gameManager.itemCreatedOldPosition == Vector3.zero && gameManager.itemCreatedOldRotation == Vector3.zero) return;

        //Kiểm tra trước khi thêm
        PreviousAction action1 = gameManager.undoActionList.previousActions.Find(x => x.action == "Move" && x.itemId == gameManager.itemIndex.itemId);
        if (action1 != null)
        {
            if (action1.position == gameObject.transform.position && action1.rotation == gameObject.transform.rotation)
            {
                return;
            }
        }

        //Lưu vào lịch sử hoạt động
        PreviousAction action = new PreviousAction();
        action.itemId = gameManager.itemIndex.itemId;
        action.action = "Move";
        action.position = gameManager.itemCreatedOldPosition;
        action.rotation.eulerAngles = gameManager.itemCreatedOldRotation;
        gameManager.undoActionList.previousActions.Add(action);

        gameManager.itemCreatedOldPosition = new Vector3();
        gameManager.itemCreatedOldRotation = new Vector3();
    }

    public float GetRotationAngle()
    {
        return transform.eulerAngles.z;
    }

    public void RotationItem(float angle)
    {
        Vector3 posA = sizePointManager.sizePointList[3].transform.position;
        Vector3 posB = sizePointManager.sizePointList[7].transform.position;
        Vector3 rotationCenter = (posA + posB) / 2;
        transform.RotateAround(rotationCenter, Vector3.forward, angle);

        if (gameManager.itemCreatedOldPosition == Vector3.zero && gameManager.itemCreatedOldRotation == Vector3.zero)
        {
            gameManager.itemCreatedOldPosition = transform.position;
            gameManager.itemCreatedOldRotation = transform.rotation.eulerAngles;
        }
    }
    public void CloneItemForConfig()
    {
        tempItem = item.DeepCopy();
    }
    public void ApplyConfigChanges()
    {
        if (tempItem != null)
        {
            item = tempItem.DeepCopy(); // Ghi lại vào bản riêng của itemCreated
            tempItem = null; // Clear tạm
        }
    }
    public void CancelConfigChanges()
    {
        tempItem = null; // Bỏ dữ liệu tạm, ko ảnh hưởng item gốc
    }
}
