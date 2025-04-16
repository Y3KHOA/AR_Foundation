using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public Transform mainCamera;            // Camera chính (AR Camera)
    public Camera miniMapCamera;            // Camera phụ (MiniMap)
    public float height = 10f;              // Độ cao của MiniMap so với camera chính
    public float smoothTime = 0.2f;         // Thời gian làm mượt vị trí
    public float iconRotationSmooth = 0.2f; // Thời gian làm mượt xoay icon

    private Vector3 velocity = Vector3.zero;
    private float currentYRotation;
    private float rotationVelocity;

    void LateUpdate()
    {
        if (mainCamera == null || miniMapCamera == null) return;

        // Vị trí mục tiêu của MiniMap Camera
        Vector3 targetPosition = mainCamera.position + Vector3.up * height;
        miniMapCamera.transform.position = Vector3.SmoothDamp(miniMapCamera.transform.position, targetPosition, ref velocity, smoothTime);

        // Làm mượt hướng quay theo Y
        float targetY = mainCamera.eulerAngles.y;
        currentYRotation = Mathf.SmoothDampAngle(currentYRotation, targetY, ref rotationVelocity, iconRotationSmooth);

        // Xoay camera phụ (nhìn từ trên xuống)
        miniMapCamera.transform.rotation = Quaternion.Euler(90f, currentYRotation, 0f);
    }
}
