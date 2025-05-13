using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SizePointEditor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public SizePointManager sizePointManager;
    public int index = -1;
    public Shared.SizePointType pointType;
    public int isShared = -1;

    private const string kindGroundString = "Kết cấu";
    private GameManager gameManager;
    private RectTransform rectTransform;
    private Camera mainCamera;
    private bool isDragging = false;
    private float minSize = 0.015f;
    private float maxSize = 0.2f;
    private Vector3 oldPosIndex = new Vector3();
    private List<Vector3> oldPosList = new List<Vector3>();

    private void Start()
    {
        gameManager = GameManager.instance;
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }

        rectTransform = GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        AdjustSizePointToCamera(); // Có thể sửa nếu muốn tối ưu hiệu suất
    }

    public void AdjustSizePointToCamera()
    {
        if (mainCamera != null)
        {
            float temp = 1;
            if (gameManager.itemIndex != null)
            {
                if(gameManager.itemIndex.item.CompareKindOfItem(kindGroundString))
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

    public void OnPointerDown(PointerEventData eventData)
    {
        gameManager.hasItem = true;
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameManager.hasItem = false;
        isDragging = false;

        SavePreviousMoveAction();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            if(oldPosIndex == Vector3.zero)
            {
                oldPosIndex = transform.localPosition;
                oldPosList = new List<Vector3>();
                for (int i = 0; i < sizePointManager.sizePointList.Count; i++)
                {
                    oldPosList.Add(Vector3.zero);
                    oldPosList[i] = sizePointManager.sizePointList[i].transform.localPosition;
                }
            }    

            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(eventData.position);
            mousePosition.z = 0;

            // Chuyển đổi vị trí chuột sang local space của đối tượng cha
            Transform parentTransform = transform.parent;
            Vector3 localMousePosition = parentTransform.InverseTransformPoint(mousePosition);

            // Khóa trục trong local space
            if (!sizePointManager.item.CompareKindOfItem(kindGroundString))
            {
                if (index == 1 || index == 5)
                {
                    localMousePosition.x = transform.localPosition.x; 
                }
                else if (index == 3 || index == 7)
                {
                    localMousePosition.y = transform.localPosition.y; 
                }
            }

            Vector3 newWorldPosition = parentTransform.TransformPoint(localMousePosition);

            sizePointManager.MoveSizePoint(index, newWorldPosition);
        }
    }

    private void SavePreviousMoveAction()
    {
        if (oldPosIndex == Vector3.zero) return;

        //Action
        PreviousAction previousAction = new PreviousAction();
        previousAction.itemId = gameManager.itemIndex.itemId;
        previousAction.action = "Change Size";
        previousAction.sizePointPosList = new List<Vector3>();
        for (int i = 0; i < sizePointManager.sizePointList.Count; i++)
        {
            previousAction.sizePointPosList.Add(Vector3.zero);
            previousAction.sizePointPosList[i] = oldPosList[i];
        }
        gameManager.undoActionList.previousActions.Add(previousAction);
        oldPosIndex = new Vector3();
    }
}
