using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameManager gameManager;
    private ItemCreated itemCreated;
    private Camera mainCamera;
    private RectTransform rectTransform;
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
        UpdatePosition();
        AdjustSizePointToCamera();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        itemCreated.isMoving = true;
        itemCreated.hasItem = true;
        gameManager.hasItem = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        itemCreated.isMoving = false;
        itemCreated.hasItem = false;
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
        average.y += distanceY + rectTransform.sizeDelta.x;
        transform.localPosition = average;
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
