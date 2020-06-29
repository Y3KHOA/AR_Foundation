using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlaceObject : MonoBehaviour
{
    [SerializeField]
    GameObject m_PlacedPrefab;
  
    ARRaycastManager m_RaycastManager;
    
    // Start is called before the first frame update
    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                var touchPosition = Input.GetTouch(0).position;
            
                List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
            
                if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycast hits are sorted by distance, so the first one
                    // will be the closest hit.
                    var hitPose = s_Hits[0].pose;
                    Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                }
            }

        }
    }
}
