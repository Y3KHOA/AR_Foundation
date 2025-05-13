using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUICanvasManager : MonoBehaviour
{
    public static GUICanvasManager instance;

    [Header("Canvas")]
    public GameObject catalogPanel;
    public GameObject boardCanvas;
    public GameObject view3dCanvas;
    public InfomationItemCanvas infomationItemCanvas;
    public GameObject categoryCanvas;
    public GameObject configCanvas;

    [Header("Banner")]
    public RectTransform wordSpaceRect;
    public RectTransform headerRect;
    public RectTransform bottomBannerRect;
    public RectTransform catalogBannerRect;
    public RectTransform categoryRect;
    public RectTransform optionRect;
    public RectTransform configRect;
    public ButtonCatalogPanel buttonCatalogPanel;

    [Header("Popup")]
    public Popup popup;

    [Header("Color Picker")]
    public GameObject colorPickerCanvas;

    [Header("Other")]
    public bool isOnWordSpace = false;
    public bool isOnCatalog = false;


    [HideInInspector] public RectTransform board2dRect;
    private GameManager gameManager;
    private Camera mainCamera;
    private RectTransform rtfCatalogBannerRect;
    private RectTransform rtfCategoryRect;
    private RectTransform rtfConfigRect;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameManager.instance;
        board2dRect = boardCanvas.GetComponent<RectTransform>();
        view3dCanvas.SetActive(false);
        infomationItemCanvas.gameObject.SetActive(false);
        popup.gameObject.SetActive(false);
        configCanvas.SetActive(false);
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
        if(wordSpaceRect == null) return;

        // Kiểm tra xem có touch hay chuột
        if (Input.touchCount > 0 || Input.GetMouseButton(0)) 
        {
            Vector2 touchPosition;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                touchPosition = touch.position;
            }
            else
            {
                touchPosition = Input.mousePosition;
            }

            rtfCatalogBannerRect = configCanvas.activeSelf ? null : catalogBannerRect;
            rtfCategoryRect = categoryCanvas.gameObject.activeSelf ? categoryRect : null;
            rtfConfigRect = configCanvas.activeSelf ? configRect : null;

            if (!float.IsInfinity(touchPosition.x) && !float.IsInfinity(touchPosition.y))
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(wordSpaceRect, touchPosition, mainCamera) &&
                    !RectTransformUtility.RectangleContainsScreenPoint(rtfCatalogBannerRect, touchPosition, mainCamera) &&
                    !RectTransformUtility.RectangleContainsScreenPoint(rtfCategoryRect, touchPosition, mainCamera) &&
                    !RectTransformUtility.RectangleContainsScreenPoint(rtfConfigRect, touchPosition, mainCamera))
                {
                    isOnWordSpace = true;
                }
                else
                {
                    isOnWordSpace = false;
                }

                ////Tắt chỉnh sửa khi chọn vào catalog
                //if (RectTransformUtility.RectangleContainsScreenPoint(rtfCatalogBannerRect, touchPosition, mainCamera))
                //{
                //    if (gameManager.itemIndex != null)
                //    {
                //        var item = gameManager.itemIndex;
                //        item.sizePointManager.EnableSizePoint(false);
                //        item.rotationBTN.gameObject.SetActive(false);
                //        item.moveBTN.gameObject.SetActive(false);
                //        item.hasItem = false;
                //        gameManager.itemIndex = null;
                //    }
                //}  
            }
            else
            {
                isOnWordSpace = false;
            }
        }
        else
        {
            isOnWordSpace = false;
        }
    }
}
