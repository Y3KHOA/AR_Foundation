using UnityEngine;

public class CompassManager : MonoBehaviour
{
    public Transform compassObject; // Mũi tên hoặc chữ "N" trong AR

    void Start()
    {
        // Bật la bàn
        Input.compass.enabled = true;
        Input.location.Start(); // Đôi khi cần bật location để la bàn hoạt động tốt hơn
    }

    void Update()
    {
        float heading = Input.compass.trueHeading;

        // Xoay mũi tên chỉ hướng Bắc
        compassObject.rotation = Quaternion.Euler(0, 0, -heading);
    }
}
