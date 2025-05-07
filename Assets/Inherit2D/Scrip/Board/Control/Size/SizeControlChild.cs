//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SizeControlChild : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
//{
//    [HideInInspector] public RectTransform rectTransform;
//    public WallChild wallChild;

//    private GameManager gameManager;
//    private bool isDragging = false;
//    private Vector3 offset;

//    void Start()
//    {
//        gameManager = GameManager.instance;
//        rectTransform = GetComponent<RectTransform>();
//        wallChild = GetComponentInParent<WallChild>();
//    }

//    // Bắt đầu kéo khi nhấn chuột hoặc touch
//    public void OnPointerDown(PointerEventData eventData)
//    {
//        gameManager.hasItem = true;
//        isDragging = true;
//        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        mousePosition.z = 0; // Đảm bảo vị trí trong mặt phẳng 2D
//        offset = transform.position - mousePosition; // Tính khoảng cách giữa chuột và đối tượng
//        wallChild.CaculateSize(mousePosition);
//    }

//    // Dừng kéo khi thả chuột hoặc touch
//    public void OnPointerUp(PointerEventData eventData)
//    {
//        gameManager.hasItem = false;
//        isDragging = false;
//        wallChild.isResizing = false;
//    }

//    // Xử lý kéo
//    public void OnDrag(PointerEventData eventData)
//    {
//        if (isDragging)
//        {
//            Vector3 mousePosition;
//            RectTransformUtility.ScreenPointToWorldPointInRectangle(
//                rectTransform, eventData.position, Camera.main, out mousePosition);
//            mousePosition.z = 0; // Đảm bảo vị trí trong mặt phẳng 2D
//            transform.position = mousePosition + offset;
//            wallChild.ControlSize(mousePosition);
//        }
//    }
//}
