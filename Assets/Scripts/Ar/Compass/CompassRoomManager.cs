using UnityEngine;
using TMPro;

public class CompassRoomManager : MonoBehaviour
{
    public GameObject compassLabelPrefab;
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
        if (compassLabelPrefab == null || distanceTextPrefab == null)
        {
            Debug.LogWarning("Chưa gán compassLabelPrefab hoặc distanceTextPrefab!");
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
            Debug.Log($"Raycast hit ARPlane: {hit.point}");
        }
        else
        {
            spawnPosition = camPos + cam.transform.forward * 0.5f - Vector3.up * 0.3f;
            Debug.LogWarning("Không raycast được mặt sàn AR, fallback tại vị trí camera.");
        }

        // Tìm tường gần nhất
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

        if (nearestWall == null)
        {
            Debug.LogWarning("Không tìm thấy tường gần nhất.");
            return;
        }

        // Tính hướng từ vị trí mũi tên đến tường
        Vector3 dirToWall = (nearestPointOnWall - spawnPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(dirToWall, Vector3.up);
        lookRotation = Quaternion.Euler(90f, lookRotation.eulerAngles.y, 90f); // ép song song mặt đất

        // Tạo mũi tên
        GameObject label = Instantiate(compassLabelPrefab, spawnPosition, lookRotation, transform);
        Debug.Log($"[Compass] Mũi tên tạo tại {spawnPosition}, hướng về tường gần nhất.");

        // Tạo vị trí text phía dưới mũi tên
        Vector3 textPosition = spawnPosition - new Vector3(0, 0.05f, 0);

        // Tạo text
        GameObject textObj = Instantiate(distanceTextPrefab, textPosition, Quaternion.identity, transform);

        // Quay text đối mặt camera theo trục Y
        Vector3 toCam = cam.transform.position - textPosition;
        toCam.y = 0;
        textObj.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);

        // Scale để đảm bảo text không quá nhỏ
        textObj.transform.localScale = Vector3.one * 0.05f;

        // Gán nội dung text
        string directionText = $"{heading:0}° {GetCompassDirectionName(heading)}";
        Debug.Log($"huong: {heading:0}° {GetCompassDirectionName(heading)}");
        Debug.Log($"huong: {heading:0}° ");

        TextMeshPro tmp = textObj.GetComponentInChildren<TextMeshPro>(true);
        if (tmp != null)
        {
            tmp.fontSize = 3f; // hoặc 5f tuỳ tầm nhìn và độ lớn bạn muốn
            tmp.text = $"{heading:0}° {GetCompassDirectionName(heading)}";
            Debug.Log("[Text] Gán vào TextMeshPro + scale font.");
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

}
