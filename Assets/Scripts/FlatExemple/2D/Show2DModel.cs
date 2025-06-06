using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Show2DModel : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject checkpointPrefab;
    public Drawing2D Drawing2D;
    public Transform modelRoot;

    [Header("Camera")]
    public Camera sceneCamera;
    public float padding = 1.2f; // Dư lề

    void Start()
    {
        LoadPointsFromDataTransfer();
        FitCameraToModel();
    }

    void LoadPointsFromDataTransfer()
    {
        List<Room> rooms = RoomStorage.rooms;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.Log("Không có Room nào trong RoomStorage.");
            return;
        }

        modelRoot.rotation = Quaternion.Euler(0f, 0, 0);

        foreach (Room room in rooms)
        {
            List<GameObject> checkpointsForRoom = new List<GameObject>();
            List<Vector2> path = room.checkpoints;

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = new Vector3(path[i].x, 0, path[i].y); // Y = 0 vì hiển thị 2D
                GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity, modelRoot);
                checkpointsForRoom.Add(checkpoint);

                if (i > 0)
                {
                    Drawing2D.DrawLineAndDistance(checkpointsForRoom[i - 1].transform.position, worldPos, modelRoot);
                }
            }

            // Tự động nối kín nếu là polygon (>= 3 điểm) và hai đầu gần nhau
            if (checkpointsForRoom.Count > 2)
            {
                Drawing2D.DrawLineAndDistance(checkpointsForRoom[^1].transform.position, checkpointsForRoom[0].transform.position, modelRoot);
            }
        }
    }

    void FitCameraToModel()
    {
        if (sceneCamera == null || modelRoot == null) return;

        // Tính bounding box của toàn bộ mô hình
        Bounds bounds = new Bounds(modelRoot.position, Vector3.zero);
        foreach (Transform child in modelRoot)
        {
            bounds.Encapsulate(child.position);
        }

        // Đặt camera nhìn từ trên xuống
        sceneCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        sceneCamera.transform.position = new Vector3(bounds.center.x, 10f, bounds.center.z); // tạm

        // Ước lượng khoảng cách camera Y cần để bao hết mô hình
        float size = Mathf.Max(bounds.size.x, bounds.size.z) * padding;
        float fovRad = sceneCamera.fieldOfView * Mathf.Deg2Rad;
        float requiredY = (size / 2f) / Mathf.Tan(fovRad / 2f);

        // Gán vị trí camera
        Vector3 newPos = sceneCamera.transform.position;
        newPos.y = requiredY;
        sceneCamera.transform.position = newPos;

        Debug.Log($"Camera đã được đặt ở Y={requiredY:F2} để bao phủ toàn bộ mô hình");
    }

}
