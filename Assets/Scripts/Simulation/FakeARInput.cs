using UnityEngine;
using UnityEngine.EventSystems;

public class FakeARInput : MonoBehaviour
{
    public Camera simCamera;
    public LayerMask planeLayer;
    public GameObject checkpointPrefab;

    void Update()
    {
        // Chỉ xử lý khi không bấm vào UI
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = simCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, planeLayer))
            {
                Vector3 pos = hit.point;
            }
        }
    }

}
