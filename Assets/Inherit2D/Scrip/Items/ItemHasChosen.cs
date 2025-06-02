using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý việc chọn và khởi tạo các đối tượng trong không gian làm việc của trò chơi.
/// </summary>
public class ItemHasChosen : MonoBehaviour
{
    [Header("Item")]
    public Item itemChosen;
    public GameObject itemCreatedPrefab;
    public GameObject item3DPrefab;

    [Header("Background")]
    public Image backGround;

    [Header("Color")]
    private Color normalColor;
    [SerializeField] private Color errorColor;
    [SerializeField] private Color succesColor;

    //Components
    private const string kindGroundString = "Kết cấu";
    private const string kindWallHangings = "Treo tường";
    private GameManager gameManager;
    [HideInInspector] public RectTransform rectTransform;
    private RectTransform board2dRect;
    private Image image;

    //Position
    private Vector3 mousePosition;
    private void Start()
    {
        gameManager = GameManager.instance;

        //Components
        rectTransform = GetComponent<RectTransform>();

        //Color
        image = GetComponent<Image>();
        normalColor = image.color;
    }

    private void Update()
    {
        if (gameManager.guiCanvasManager.isOnWordSpace)
        {
            //if (itemHasChosen.kindOfItem == "Draw")
            //{
            //    return;
            //}

            //Điều chỉnh vị trí chuột
            SetMousePosition();
            SetPosition();
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, -2f);
        }
    }

    //Kích hoạt khi nhấn chọn đối tượng trong canvas
    public void HaveItem(Item item)
    {
        itemChosen = item;
        gameManager.hasItem = true;

        // Đặt kích thước width và height bằng cách sử dụng sizeDelta
        rectTransform.sizeDelta = new Vector2(item.length, item.width) * 10;
    }

    //Khởi tạo đối tượng
    public void InitItem()
    {
        //2D
        gameManager.numberOfItemsCreated++;
        ItemCreated itemCreated = Instantiate(itemCreatedPrefab, gameManager.createdItems2DParent.transform).GetComponent<ItemCreated>();
        itemCreated.itemId = gameManager.numberOfItemsCreated;
        itemCreated.item = itemChosen.DeepCopy(itemChosen);
        if (itemChosen.CompareKindOfItem(kindGroundString))
        {
            itemCreated.rectTransform.localPosition = itemCreated.transform.parent.InverseTransformPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, -0.5f));
        }
        else
        {
            itemCreated.rectTransform.localPosition = itemCreated.transform.parent.InverseTransformPoint(rectTransform.position);
        }

        itemCreated.sizePointManager.item = itemCreated.item;
        itemCreated.sizePointManager.DrawOutline(itemCreated.item);
        itemCreated.sizePointManager.CreateSizePoints();
        itemCreated.sizePointManager.EnableSizePoint(false);
        itemCreated.sizePointManager.EnableEdgeText(false);
        gameManager.createdItems2DList.Add(itemCreated);

        if (itemChosen.CompareKindOfItem(kindGroundString)) gameManager.createdGroudList.Add(itemCreated);

        //3D
        Vector3 pos = new Vector3(mousePosition.x, mousePosition.y, -6 * itemCreated.item.distance);
        Box3D item3d = Instantiate(item3DPrefab, pos, transform.rotation, gameManager.createdItems3DParent.transform).GetComponent<Box3D>();
        item3d.transform.localScale = new Vector3(itemCreated.item.length * 10f, itemCreated.item.width * 10f, itemCreated.item.height * 10f);
        item3d.index = itemCreated.itemId;
        gameManager.createdItems3DList.Add(item3d);

        //Lưu vào lịch sử hoạt động
        PreviousAction action = new PreviousAction();
        action.itemId = itemCreated.itemId;
        action.action = "Create";
        action.position = transform.position;
        action.rotation.eulerAngles = new Vector3();
        gameManager.undoActionList.previousActions.Add(action);

        //
        gameManager.hasItem = false;
    }

    public void SetMousePosition()
    {
        board2dRect = gameManager.guiCanvasManager.board2dRect;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            board2dRect,
            Input.mousePosition,
            Camera.main,
            out mousePosition
        );
    }

    public void SetPosition()
    {
        if (!itemChosen.CompareKindOfItem(kindWallHangings))
        {
            gameObject.transform.position = mousePosition;
        }
        else
        {
            SetPositionWallHangings();
        }
    }

    private void SetPositionWallHangings()
    {
        if (gameManager.createdGroudList.Count == 0) return;

        SetMousePosition();
        Vector3 mouseWorldPosition = mousePosition;

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
            gameObject.transform.position = nearestCollider.ClosestPoint(mouseWorldPosition);
            gameObject.transform.rotation = Quaternion.Euler(nearestCollider.transform.eulerAngles);
        }
    }

    public void CancelChosenItem()
    {
        if (gameManager.itemHasChosen.gameObject.activeSelf)
        {
            gameManager.itemHasChosen.itemChosen = null;
            gameManager.itemHasChosen.gameObject.SetActive(false);
        }
    }
}
