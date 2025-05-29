using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Lớp này quản lý việc kéo và thả các item trong không gian làm việc của trò chơi.
/// </summary>
public class ItemCanvas : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("Item")]
    public Item item;

    [Header("Text Mesh Pro")]
    public TextMeshProUGUI itemNameText;
    public Image image;

    private GameManager gameManager;
    private Item tempItem;

    private ScrollRect scrollRect; // Tham chiếu đến danh sách
    private bool isVerticalDrag = false; // Kiểm tra hướng kéo
    private bool isDragging = false;    // Đang kéo item
    private Vector3 mousePosTemp = new Vector3();

    private void Start()
    {
        gameManager = GameManager.instance;

        // Tìm ScrollRect trên danh sách cha
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    public void LoadData()
    {
        itemNameText.text = item.itemName;
        LoadImage(item.imageName);
    }

    private void LoadImage(string imageName)
    {
        Sprite sprite = Resources.Load<Sprite>($"ImagesItem/{imageName}");
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogError($"Failed to load image '{imageName}'");
            image.sprite = null;
        }
    }

    public void ResetData()
    {
        item = null;
        itemNameText.text = "";
        image.sprite = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameManager.isLock) return;

        isVerticalDrag = false;
        isDragging = false;

        float absX = Mathf.Abs(eventData.delta.x);
        float absY = Mathf.Abs(eventData.delta.y);

        if (absY > absX)
        {
            isVerticalDrag = true;

            if (scrollRect != null)
            {
                scrollRect.OnBeginDrag(eventData);
            }
        }
        else
        {
            gameManager.hasItem = true;
            isVerticalDrag = false;
            tempItem = item;

            if (gameManager.itemIndex != null)
            {
                var item = gameManager.itemIndex;
                item.sizePointManager.EnableSizePoint(false);
                item.rotationBTN.gameObject.SetActive(false);
                item.moveBTN.gameObject.SetActive(false);
                item.hasItem = false;
                gameManager.itemIndex = null;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (gameManager.isLock) return;

        if (isVerticalDrag)
        {
            if (scrollRect != null)
            {
                scrollRect.OnDrag(eventData); //Nhận event
            }
        }
        else if (gameManager.guiCanvasManager.isOnWordSpace || gameManager.hasItem)
        {
            if (!item.CompareKindOfItem("Thao tác"))
            {
                isDragging = true;
                gameManager.itemHasChosen.gameObject.SetActive(true);
                gameManager.itemHasChosen.SetMousePosition();
                gameManager.itemHasChosen.SetPosition();

                gameManager.guiCanvasManager.buttonCatalogPanel.openCatalogButton.image.sprite = gameManager.guiCanvasManager.buttonCatalogPanel.normalSprite;
                gameManager.guiCanvasManager.categoryCanvas.SetActive(false);
                gameManager.itemHasChosen.HaveItem(tempItem);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (gameManager.isLock) return;

        if (isVerticalDrag)
        {
            if (scrollRect != null)
            {
                scrollRect.OnEndDrag(eventData); //Trả event
            }
        }
        else if (isDragging && gameManager.guiCanvasManager.isOnWordSpace)
        {
            gameManager.itemHasChosen.InitItem();
            gameManager.itemHasChosen.gameObject.SetActive(false);
        }
        else if (isDragging && !gameManager.guiCanvasManager.isOnWordSpace)
        {
            gameManager.hasItem = false;
            gameManager.itemHasChosen.itemChosen = null;
            gameManager.itemHasChosen.gameObject.SetActive(false);
        }

        gameManager.hasItem = false;
        isVerticalDrag = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        mousePosTemp = Input.mousePosition;
    }

    public void DrawWallOnClick()
    {
        if (mousePosTemp == Input.mousePosition)
        {
            if (item.CompareKindOfItem("Thao tác"))
            {
                if (gameManager.itemIndex == null)
                {
                    if (gameManager.guiCanvasManager.popup.gameObject.activeSelf)
                    {
                        gameManager.guiCanvasManager.popup.animator.SetInteger("state", 0);
                        gameManager.guiCanvasManager.popup.gameObject.SetActive(false);
                    }

                    gameManager.guiCanvasManager.popup.gameObject.SetActive(true);
                    gameManager.guiCanvasManager.popup.image.sprite = image.sprite;
                    gameManager.guiCanvasManager.popup.textMeshProUGUI.text = "Vui lòng chọn nền cần thao tác trước";
                }
                else
                {
                    if (!gameManager.itemIndex.item.CompareKindOfItem("Kết cấu")) return;

                    if (gameManager.guiCanvasManager.popup.gameObject.activeSelf)
                    {
                        gameManager.guiCanvasManager.popup.animator.SetInteger("state", 0);
                        gameManager.guiCanvasManager.popup.gameObject.SetActive(false);
                    }

                    gameManager.ActivateDrawing();
                }
            }
            else
            {
                if (gameManager.guiCanvasManager.popup.gameObject.activeSelf)
                {
                    gameManager.guiCanvasManager.popup.animator.SetInteger("state", 0);
                    gameManager.guiCanvasManager.popup.gameObject.SetActive(false);
                }

                gameManager.guiCanvasManager.popup.gameObject.SetActive(true);
                gameManager.guiCanvasManager.popup.image.sprite = image.sprite;
                gameManager.guiCanvasManager.popup.textMeshProUGUI.text = "Giữ và kéo sang trái để thêm";
            }
        }
    }
}
