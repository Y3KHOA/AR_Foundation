using UnityEngine;

public class MiniMap3D : MonoBehaviour
{
    public Transform mainCamera; // Camera chính của AR
    public RectTransform miniMapUI; // UI chứa MiniMap3D
    public Transform miniMapObjects; // Chứa tất cả đối tượng thu nhỏ

    public float scaleFactor = 0.5f; // Tỉ lệ thu nhỏ MiniMap
    public Vector3 uiOffset = new Vector3(-200, -200, 0); // Vị trí MiniMap trên màn hình
    public GameObject prefab3D; // Prefab 3D muốn thêm vào MiniMap

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main.transform;

        if (miniMapUI != null)
        {
            // Đặt vị trí MiniMap ở góc phải
            miniMapUI.anchoredPosition = new Vector2(Screen.width / 2 - 100, Screen.height / 2 - 100);
        }

        // Gọi hàm thêm prefab vào MiniMap
        if (prefab3D != null)
        {
            AddToMiniMap(prefab3D);
        }
    }

    void Update()
    {
        if (miniMapObjects != null)
        {
            // Xoay MiniMap theo góc của Camera nhưng giữ hướng nhìn từ trên xuống
            miniMapObjects.rotation = Quaternion.Euler(90, mainCamera.eulerAngles.y, 0);
        }
    }

    // Hàm để thêm đối tượng vào MiniMap (Gọi khi tạo điểm/tường)
    public void AddToMiniMap(GameObject original)
    {
        GameObject miniClone = Instantiate(original, miniMapObjects);
        miniClone.transform.localScale *= scaleFactor; // Thu nhỏ mô hình
        miniClone.transform.position = ConvertToMiniMapPosition(original.transform.position);
        Debug.Log("Added prefab to MiniMap at: " + miniClone.transform.position);
    }

    // Chuyển đổi vị trí thực tế sang vị trí MiniMap
    private Vector3 ConvertToMiniMapPosition(Vector3 realPosition)
    {
        // Nếu cần, có thể thử thêm một biến offset để điều chỉnh
        float x = realPosition.x * scaleFactor;
        float z = realPosition.z * scaleFactor;
        return new Vector3(x, 0, z);
    }
}
