using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialHandle : MonoBehaviour
{
    public static TutorialBase tutorial;
    private Camera mainCam;
    public GameObject inputPanel;
    [SerializeField] private float originalSize = 5;
    [SerializeField] private float ratio = 5;
    [SerializeField] private Vector3 offset;

    private Vector3 originalScale = Vector3.one;
    
    public float Ratio => ratio;
    
    public void Play()
    {
        tutorial.transform.position = GetCenterPos(tutorial.transform) + offset;
        ProcessRatio();
        tutorial.SetRatio(ratio);
        tutorial?.PlayTutorial();
        inputPanel.gameObject.SetActive(true);
    }

    public void Stop()
    {
        tutorial?.StopTutorial();
        inputPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        mainCam = Camera.main;
        originalScale = tutorial.transform.localScale;
        originalSize = mainCam.orthographicSize;
        Play();
    }

    private void ProcessRatio()
    {
        ratio = mainCam.orthographicSize / originalSize;
        tutorial.transform.localScale = originalScale * ratio;
    }
    
    private void Update()
    {
        SetToCenterOfCamera();
    }

    private void SetToCenterOfCamera()
    {
        var center = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
    }
    
    
    private Vector3 GetCenterPos(Transform target)
    {
        var center = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
        return new Vector3(center.x, target.transform.position.y, center.z);
    }
}