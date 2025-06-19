using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class StableRaycastWithAnchor : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public ARAnchorManager anchorManager;

    public GameObject ghostPrefab;
    public GameObject placedPrefab;

    private GameObject ghostInstance;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        if (ghostPrefab != null)
        {
            ghostInstance = Instantiate(ghostPrefab);
            ghostInstance.SetActive(false);
        }
    }

    void Update()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            if (ghostInstance != null)
            {
                ghostInstance.SetActive(true);
                ghostInstance.transform.SetPositionAndRotation(pose.position, pose.rotation);
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ARPlane plane = planeManager.GetPlane(hits[0].trackableId);
                ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);

                if (anchor == null)
                {
                    Debug.LogError("Không thể tạo Anchor!");
                    return;
                }

                Instantiate(placedPrefab, anchor.transform);
            }
        }
        else
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
        }
    }
}
