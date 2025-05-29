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
        float heading = CompassManager.Instance.GetCurrentHeading();

        // Lấy camera và vị trí raycast
        Camera cam = Camera.main ?? (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
        if (cam == null)
        {
            Debug.LogError("Không tìm thấy camera.");
            return;
        }

        Vector3 rayPos;
        if (Physics.Raycast(new Ray(cam.transform.position, Vector3.down), out RaycastHit hit, 10f))
            rayPos = hit.point;
        else
            rayPos = cam.transform.position + cam.transform.forward * 0.5f;

        // Tìm index tường gần nhất
        int nearestIndex = FindNearestWallIndex(currentRoom, rayPos);

        if (nearestIndex >= 0)
        {
            while (currentRoom.headingCompass.Count <= nearestIndex)
                currentRoom.headingCompass.Add(-1f);

            currentRoom.headingCompass[nearestIndex] = heading;
            Debug.Log($"Gán heading {heading:0.0}° cho wallLines[{nearestIndex}]");

            CreateCompassLabel(heading, currentRoom.wallLines[nearestIndex], rayPos);
        }
        else
        {
            Debug.LogWarning("Không xác định được đoạn tường gần nhất.");
        }
    }

    private int FindNearestWallIndex(Room room, Vector3 referencePos)
    {
        int nearestIndex = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < room.wallLines.Count; i++)
        {
            WallLine wall = room.wallLines[i];
            Vector3 nearestPoint = ClosestPointOnLine(wall.start, wall.end, referencePos);
            float dist = Vector3.Distance(referencePos, nearestPoint);

            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    private void CreateCompassLabel(float heading, WallLine wall, Vector3 spawnPosition)
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

        // Tìm điểm gần nhất trên tường
        Vector3 nearestPointOnWall = ClosestPointOnLine(wall.start, wall.end, spawnPosition);
        Vector3 dirToWall = (nearestPointOnWall - spawnPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(dirToWall, Vector3.up);
        lookRotation = Quaternion.Euler(90f, lookRotation.eulerAngles.y, 90f);

        // Tạo mũi tên
        GameObject label = Instantiate(compassLabelPrefab, spawnPosition, lookRotation, transform);
        Debug.Log($"[Compass] Mũi tên tạo tại {spawnPosition}, hướng về tường gần nhất.");

        // Tạo vị trí text phía dưới mũi tên
        Vector3 textPosition = spawnPosition - new Vector3(0, 0.05f, 0);

        GameObject textObj = Instantiate(distanceTextPrefab, textPosition, Quaternion.identity, transform);

        // Quay text đối mặt camera
        Vector3 toCam = cam.transform.position - textPosition;
        toCam.y = 0;
        textObj.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        textObj.transform.localScale = Vector3.one * 0.05f;

        string directionText = $"{heading:0}° {GetCompassDirectionName(heading)}";
        TextMeshPro tmp = textObj.GetComponentInChildren<TextMeshPro>(true);
        if (tmp != null)
        {
            tmp.fontSize = 3f;
            tmp.text = directionText;
            Debug.Log($"[Text] Gán vào TMP: {directionText}");
        }
    }

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
