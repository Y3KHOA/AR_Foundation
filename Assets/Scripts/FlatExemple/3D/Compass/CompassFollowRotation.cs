using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CompassFollowRotation : MonoBehaviour
{
    public Transform roomModel;
    public Image compassImage;
    public TextMeshProUGUI compassText;

    private Room currentRoom;

    void Start()
    {
        List<Room> rooms = RoomStorage.rooms;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("Không có Room nào trong RoomStorage.");
            return;
        }

        // Giả sử roomModel ứng với phòng đầu tiên — có thể thay đổi logic này nếu cần
        currentRoom = rooms[0];
    }

    void Update()
    {
        if (roomModel == null || compassImage == null || currentRoom == null)
            return;

        WallLine facingWall = GetMostFacingWall(currentRoom);
        if (facingWall != null)
        {
            Vector3 dir = (facingWall.end - facingWall.start).normalized;
            float angleToNorth = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float realWorldAngle = (angleToNorth + currentRoom.headingCompass + 360f) % 360f;

            float yRotation = NormalizeAngle(roomModel.eulerAngles.y);
            yRotation = realWorldAngle;
            compassImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, yRotation);

            string label = AngleToDirectionLabel(realWorldAngle);
            compassText.text = $"{realWorldAngle:F1}° ({label})";
        }
        else
        {
            compassText.text = "N/A";
        }
    }

    WallLine GetMostFacingWall(Room room)
    {
        if (Camera.main == null || room.wallLines == null || room.wallLines.Count == 0)
            return null;

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        float maxDot = -1f;
        WallLine bestWall = null;

        foreach (var wall in room.wallLines)
        {
            Vector3 wallDir = (wall.end - wall.start).normalized;
            wallDir.y = 0;
            float dot = Vector3.Dot(wallDir, cameraForward);  // Cosine góc giữa hướng tường và camera

            if (dot > maxDot)
            {
                maxDot = dot;
                bestWall = wall;
            }
        }
        return bestWall;
    }
    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        return angle < 0f ? angle + 360f : angle;
    }
    private void UpdateWallDirections(Room room)
    {
        foreach (WallLine line in room.wallLines)
        {
            Vector3 dir = (line.end - line.start).normalized;

            // Góc so với trục Z (Bắc)
            float angleToNorth = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // Cộng với góc chuẩn la bàn
            float realWorldAngle = (angleToNorth + room.headingCompass + 360f) % 360f;

            // Gợi ý hướng chữ
            string directionLabel = AngleToDirectionLabel(realWorldAngle);

            Debug.Log($"[list][WallDir] {line.start} → {line.end} = {realWorldAngle:0.0}° ({directionLabel})");
        }
    }

    private string AngleToDirectionLabel(float degree)
    {
        if (degree < 0) degree += 360;

        if ((degree >= 0 && degree < 7.5f) || degree >= 352.5f) return "Bắc";
        if (degree < 22.5f) return "Bắc";
        if (degree < 37.5f) return "Đông Bắc";
        if (degree < 52.5f) return "Đông Bắc";
        if (degree < 67.5f) return "Đông Bắc";
        if (degree < 82.5f) return "Đông";
        if (degree < 97.5f) return "Đông";
        if (degree < 112.5f) return "Đông";
        if (degree < 127.5f) return "Đông Nam";
        if (degree < 142.5f) return "Đông Nam";
        if (degree < 157.5f) return "Đông Nam";
        if (degree < 172.5f) return "Nam";
        if (degree < 187.5f) return "Nam";
        if (degree < 202.5f) return "Nam";
        if (degree < 217.5f) return "Tây Nam";
        if (degree < 232.5f) return "Tây Nam";
        if (degree < 247.5f) return "Tây Nam";
        if (degree < 262.5f) return "Tây";
        if (degree < 277.5f) return "Tây";
        if (degree < 292.5f) return "Tây";
        if (degree < 307.5f) return "Tây Bắc";
        if (degree < 322.5f) return "Tây Bắc";
        if (degree < 337.5f) return "Tây Bắc";
        return "Bắc";
    }
}
