using UnityEngine;
using TMPro;

public class CompassRoomManager : MonoBehaviour
{
    public GameObject compassLabelPrefab;
    public GameObject compassLabelPrefab2;
    public GameObject distanceTextPrefab;

    public void OnSetCompassDirectionForCurrentRoom()
    {
        Debug.Log(">>> Button clicked: gan huong phong");

        if (RoomStorage.rooms == null || RoomStorage.rooms.Count == 0)
        {
            Debug.LogWarning("Button clicked: no room in RoomStorage.");
            return;
        }

        Room currentRoom = RoomStorage.rooms[RoomStorage.rooms.Count - 1];
        float heading = CompassManager.Instance.GetCurrentHeading(); // lấy từ compass mượt

        currentRoom.headingCompass = heading;
        Debug.Log($"[Set Heading] {heading:0.0}° for room");
        // In ra hướng và vị trí hiện tại đã lưu
        Debug.Log($"[Room Info] Compass Heading: {currentRoom.headingCompass:0.0}, Position: {currentRoom.Compass}");

        // Tìm trung tâm phòng để đặt nhãn
        Vector3 center = GetRoomCenter(currentRoom);

        // Tạo nhãn hoặc mũi tên hướng
        CreateCompassLabel(heading);
    }
    private Vector3 GetRoomCenter(Room room)
    {
        if (room.checkpoints == null || room.checkpoints.Count == 0)
            return Vector3.zero;

        float sumX = 0, sumY = 0;
        foreach (var pt in room.checkpoints)
        {
            sumX += pt.x;
            sumY += pt.y;
        }

        float centerX = sumX / room.checkpoints.Count;
        float centerZ = sumY / room.checkpoints.Count;
        return new Vector3(centerX, 0, centerZ);
    }

    private void CreateCompassLabel(float heading)
    {
        if (compassLabelPrefab == null)
        {
            Debug.LogWarning("Chưa gán compassLabelPrefab!");
            return;
        }

        Camera cam = Camera.main != null ? Camera.main : Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null;
        if (cam == null)
        {
            Debug.LogError("Không tìm thấy Camera.main");
            return;
        }

        Room currentRoom = RoomStorage.rooms[RoomStorage.rooms.Count - 1];

        Vector3 camPos = cam.transform.position;
        Ray ray = new Ray(camPos, Vector3.down);
        Vector3 spawnPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            spawnPosition = hit.point + Vector3.up * 0.1f;
            currentRoom.Compass = new Vector2(spawnPosition.x, spawnPosition.z);
            Debug.Log($"Raycast hit ARPlane: {hit.point}");
        }
        else
        {
            spawnPosition = camPos + cam.transform.forward * 0.5f - Vector3.up * 0.3f;
            currentRoom.Compass = new Vector2(spawnPosition.x, spawnPosition.z);
            Debug.LogWarning("Không raycast được mặt sàn AR, fallback tại vị trí camera.");
        }

        // Xác định tường gần nhất
        WallLine nearestWall = null;
        Vector3 nearestPointOnWall = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (var wall in currentRoom.wallLines)
        {
            Vector3 closest = ClosestPointOnLine(wall.start, wall.end, spawnPosition);
            float dist = Vector3.Distance(spawnPosition, closest);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestWall = wall;
                nearestPointOnWall = closest;
            }
        }

        if (nearestWall != null)
        {
            Quaternion lookRotation = Quaternion.Euler(90f, heading, 135f); // Quay quanh trục Y theo hướng la bàn

            Debug.Log("[0] huong mui ten: " + lookRotation.eulerAngles);
            Debug.Log("[0] huong mui ten: " + lookRotation);

            // Xoá tất cả mũi tên cũ
            foreach (Transform child in transform)
            {
                if (child.name.Contains("CompassLabel"))
                {
                    Destroy(child.gameObject);
                }
            }
            foreach (Transform child in transform)
            {
                if (child.name.Contains("CompassLabel2"))
                {
                    Destroy(child.gameObject);
                }
            }

            GameObject label = Instantiate(
                compassLabelPrefab,
                spawnPosition,
                lookRotation,
                transform
            );
            label.name = "CompassLabel";
            GameObject label2 = Instantiate(
                compassLabelPrefab2,
                spawnPosition,
                lookRotation,
                transform
            );
            label2.name = "CompassLabel2";

            SetLayerRecursively(label2, LayerMask.NameToLayer("PreviewModel"));
        }
        else
        {
            Debug.LogWarning("Không tìm thấy tường gần nhất để hướng mũi tên.");
        }
    }

    // Hàm bổ trợ tìm điểm gần nhất trên đoạn thẳng
    private Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }


    private string GetCompassDirectionName(float heading)
    {
        string[] dirs = { "Bắc", "Đông Bắc", "Đông", "Đông Nam", "Nam", "Tây Nam", "Tây", "Tây Bắc" };
        int index = Mathf.RoundToInt(heading / 45f) % 8;
        return dirs[index];
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
