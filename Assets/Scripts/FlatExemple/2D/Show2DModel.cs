using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Show2DModel : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject checkpointPrefab;
    public Drawing2D Drawing2D;
    public Transform modelRoot;

    [Header("Camera")]
    public Camera sceneCamera;
    public float padding = 1.2f; // Dư lề

    [Header("Buttons")]
    public Button ButtonFloorPlan;
    public Button Button3DView;
    public Button ButtonInfo;

    void Start()
    {
        LoadPointsFromRoomStorage();
        SetupButtons();   // Gắn listener cho nút
        FitCameraToFloorPlan(); // Mặc định mở FloorPlan
    }

    void SetupButtons()
    {
        ButtonFloorPlan.onClick.AddListener(FitCameraToFloorPlan);
        Button3DView.onClick.AddListener(FitCameraTo3DView);
        ButtonInfo.onClick.AddListener(FitCameraToInfo);
    }

    void LoadPointsFromRoomStorage()
    {
        List<Room> rooms = RoomStorage.rooms;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.Log("Không có Room nào trong RoomStorage.");
            return;
        }

        modelRoot.rotation = Quaternion.identity;

        foreach (Room room in rooms)
        {
            List<GameObject> checkpointsForRoom = new List<GameObject>();
            List<Vector2> path = room.checkpoints;

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = new Vector3(path[i].x, 0, path[i].y);
                GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity, modelRoot);
                checkpointsForRoom.Add(checkpoint);

                if (i > 0)
                {
                    Drawing2D.DrawLineAndDistance(checkpointsForRoom[i - 1].transform.position, worldPos, modelRoot);
                }
            }

            if (checkpointsForRoom.Count > 2)
            {
                Drawing2D.DrawLineAndDistance(
                    checkpointsForRoom[^1].transform.position,
                    checkpointsForRoom[0].transform.position,
                    modelRoot
                );
            }

            Debug.Log($"[LoadPoints] Loaded Room ID: {room.ID} với {path.Count} điểm");
        }
    }

    Bounds GetModelBounds()
    {
        Bounds bounds = new Bounds(modelRoot.position, Vector3.zero);
        foreach (Transform child in modelRoot)
            bounds.Encapsulate(child.position);
        return bounds;
    }

    void FitCameraToFloorPlan()
    {
        Bounds bounds = GetModelBounds();
        float size = Mathf.Max(bounds.size.x, bounds.size.z) * padding;
        float fovRad = sceneCamera.fieldOfView * Mathf.Deg2Rad;
        float requiredY = (size / 2f) / Mathf.Tan(fovRad / 2f);

        sceneCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        sceneCamera.transform.position = new Vector3(bounds.center.x, requiredY, bounds.center.z);

        Debug.Log("[Camera] Floor Plan: Top-down view");
    }

    void FitCameraTo3DView()
    {
        Bounds bounds = GetModelBounds();
        float size = Mathf.Max(bounds.size.x, bounds.size.z) * padding;
        float fovRad = sceneCamera.fieldOfView * Mathf.Deg2Rad;
        float requiredY = (size / 2f) / Mathf.Tan(fovRad / 2f);

        sceneCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        sceneCamera.transform.position = new Vector3(bounds.center.x, requiredY / 2f, bounds.center.z - 10f);

        Debug.Log("[Camera] 3D View: Perspective");
    }

    void FitCameraToInfo()
    {
        Bounds bounds = GetModelBounds();
        float size = Mathf.Max(bounds.size.x, bounds.size.z) * padding;
        float fovRad = sceneCamera.fieldOfView * Mathf.Deg2Rad;
        float requiredY = (size / 2f) / Mathf.Tan(fovRad / 2f);

        sceneCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        sceneCamera.transform.position = new Vector3(bounds.center.x, requiredY / 2f, bounds.center.z + 10f);

        Debug.Log("[Camera] Info View: Perspective");
    }
}
