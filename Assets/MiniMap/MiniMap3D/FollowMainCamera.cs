using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
    public Camera mainCamera;  // Camera chính (ARCamera)
    public Camera secondaryCamera;  // Camera phụ (MiniMap Camera)

    public Vector3 offset = new Vector3(0f, 10f, 0f);  // Khoảng cách giữa camera chính và camera phụ
    public float followSpeed = 5f;  // Tốc độ di chuyển của camera phụ

    void Start()
    {
        // Kiểm tra xem camera chính và camera phụ có được gán chưa
        if (mainCamera == null || secondaryCamera == null)
        {
            Debug.LogError("Chưa gán camera chính hoặc camera phụ.");
            return;
        }

        // Thiết lập camera phụ ban đầu ở vị trí offset so với camera chính
        secondaryCamera.transform.position = mainCamera.transform.position + offset;

        // Đảm bảo camera phụ nhìn xuống mặt đất (góc nhìn thích hợp cho miniMap)
        secondaryCamera.transform.rotation = Quaternion.Euler(45f, mainCamera.transform.eulerAngles.y, 0f); // Góc 45 độ theo trục X
    }

    void Update()
    {
        // Cập nhật vị trí của camera phụ để luôn theo camera chính
        if (mainCamera != null && secondaryCamera != null)
        {
            // Lấy vị trí mới cho camera phụ theo offset
            Vector3 newPos = mainCamera.transform.position + offset;

            // Di chuyển camera phụ với tốc độ mượt mà nhưng giữ trục Z cố định
            newPos.z = secondaryCamera.transform.position.z;

            // Di chuyển camera phụ tới vị trí mới
            secondaryCamera.transform.position = Vector3.Lerp(secondaryCamera.transform.position, newPos, followSpeed * Time.deltaTime);

            // Camera phụ xoay theo góc của camera chính, giữ góc nhìn xuống (45 độ) nhưng không thay đổi trục Z
            secondaryCamera.transform.rotation = Quaternion.Euler(45f, mainCamera.transform.eulerAngles.y, 0f);  // Xoay góc 45 độ theo trục X
        }
    }
}
