using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // mainCamera = Camera.main; // Lấy camera chính
        mainCamera = FindObjectOfType<Camera>(); // Tìm bất kỳ camera nào trong Scene
    }

    void Update()
    {
        // if (mainCamera != null)
        // {
        //     transform.LookAt(mainCamera.transform); // Nhìn về phía camera
        //     transform.Rotate(0, 180, 0); // Quay ngược lại để tránh bị ngược chữ
        // }
    }
}
