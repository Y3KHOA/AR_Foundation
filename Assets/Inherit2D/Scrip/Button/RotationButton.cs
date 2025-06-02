using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lớp này xử lý việc xoay một vật phẩm trong trò chơi. Nó cho phép người dùng xoay vật phẩm bằng cách nhấp và kéo, cập nhật vị trí và kích thước của vật phẩm dựa trên khoảng cách camera.
/// </summary>
public class RotationButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameManager gameManager;
    private ItemCreated itemCreated;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private bool isPressing;
    private float initialMouseAngle;
    private Vector3 parentPosition;
    private const string kindGroundString = "Kết cấu";
    private float minSize = 0.015f;
    private float maxSize = 0.215f;

    public void Start()
    {
        gameManager = GameManager.instance;
        itemCreated = GetComponentInParent<ItemCreated>();
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        Vector3 posX = (itemCreated.sizePointManager.sizePointList[3].transform.position + itemCreated.sizePointManager.sizePointList[7].transform.position) / 2;
        parentPosition = posX;

        if (isPressing)
        {
            Vector3 currentMousePos = Input.mousePosition;
            currentMousePos = Camera.main.ScreenToWorldPoint(currentMousePos);
            Vector3 direction = currentMousePos - parentPosition;

            float currentMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float deltaAngle = currentMouseAngle - initialMouseAngle;
            itemCreated.RotationItem(deltaAngle);
            initialMouseAngle = currentMouseAngle;
        }

        UpdatePosition();
        UpdateText();
        AdjustSizePointToCamera();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        itemCreated.isRotate = true;
        gameManager.hasItem = true;

        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = mousePos - parentPosition;
        initialMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        itemCreated.isRotate = false;
        gameManager.hasItem = false;
    }

    public void UpdatePosition()
    {
        Vector3 posA = itemCreated.sizePointManager.sizePointList[3].transform.localPosition;
        Vector3 posB = itemCreated.sizePointManager.sizePointList[7].transform.localPosition;
        Vector3 posC = itemCreated.sizePointManager.sizePointList[1].transform.localPosition;
        Vector3 posD = itemCreated.sizePointManager.sizePointList[5].transform.localPosition;

        Vector3 average = (posA + posB) / 2;
        float distanceY = Vector3.Distance(posC, posD) / 1.42f;
        average.y += -distanceY - rectTransform.sizeDelta.x;
        transform.localPosition = average;
    }

    public void UpdateText()
    {
        if (Camera.main != null)
        {
            itemCreated.sizePointManager.areaText.transform.LookAt(Camera.main.transform);
            itemCreated.sizePointManager.areaText.transform.Rotate(0, 180, 0);

            for (int i = 0; i < 4; i++)
            {
                itemCreated.sizePointManager.edgeLengthTextObjects[i].transform.LookAt(Camera.main.transform);
                itemCreated.sizePointManager.edgeLengthTextObjects[i].transform.Rotate(0, 180, 0);
            }
        }
    }

    public void AdjustSizePointToCamera()
    {
        if (mainCamera != null)
        {
            float temp = 1;
            if (gameManager.itemIndex != null)
            {
                if (gameManager.itemIndex.item.CompareKindOfItem(kindGroundString))
                {
                    temp = 2;
                }
                else
                {
                    temp = 1.5f;
                }
            }
            float screenHeight = Screen.height;
            float cameraSize = mainCamera.orthographicSize;
            float scaleFactor = cameraSize / screenHeight;
            scaleFactor = Mathf.Clamp(scaleFactor, minSize, maxSize);

            rectTransform.sizeDelta = new Vector2(50, 50) * scaleFactor * temp;
        }
    }
}
