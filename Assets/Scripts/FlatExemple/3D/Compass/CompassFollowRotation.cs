using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompassFollowRotation : MonoBehaviour
{
    public Transform roomModel;       // Mô hình cần theo dõi
    public Image compassImage;        // Hình ảnh UI của la bàn
    public TextMeshProUGUI compassText;   // Text hiển thị độ đã xoay
    
    void Update()
    {
        if (roomModel == null || compassImage == null)
            return;

        // Lấy rotation theo trục y của mô hình
        float yRotation = NormalizeAngle(roomModel.eulerAngles.y);

        // Chuyển đổi giá trị X thành góc xoay Z của compass
        compassImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, yRotation);

        // Hiển thị góc với 2 chữ số sau dấu phẩy
        compassText.text = $"{yRotation:F2}°";
    }
    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        return angle < 0f ? angle + 360f : angle;
    }
}
