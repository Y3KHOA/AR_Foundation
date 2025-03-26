using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneDetectionController : MonoBehaviour
{
    private ARPlaneManager planeManager;

    void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();

        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager khong tim thay! Kiem tra XR Origin.");
            return;
        }

        if (planeManager.subsystem != null && planeManager.subsystem.subsystemDescriptor.supportsVerticalPlaneDetection)
        {
            Debug.Log("Thiet bi ho tro nhan dien mat phang dung (tuong).");
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
            Debug.Log("Da bat nhan dien mat phang ngang va dung.");
        }
        else
        {
            Debug.LogWarning("Thiet bi nay khong ho tro nhan dien tuong hoac mat phang doc.");
        }
    }
}
