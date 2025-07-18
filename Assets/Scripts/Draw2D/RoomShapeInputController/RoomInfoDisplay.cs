using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RoomInfoDisplay : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text lengthText;
    public TMP_Text widthText;
    public TMP_Text perimeterText;
    public TMP_Text areaText;

    [Header("Reference")]
    private CheckpointManager checkpointManager; // Tham chiếu đến CheckpointManager để điều khiển room đã chọn

    private string selectedRoomID = "";


    void Start()
    {        
        checkpointManager = FindFirstObjectByType<CheckpointManager>();
    }

    void Update()
    {
        string currentRoomID = checkpointManager.GetSelectedRoomID();

        // Nếu trước đó có chọn mà giờ không chọn gì ➜ clear
        if (!string.IsNullOrEmpty(selectedRoomID) && string.IsNullOrEmpty(currentRoomID))
        {
            selectedRoomID = "";
            ClearText();
            return;
        }

        // Nếu có RoomID đang chọn → luôn cập nhật thông tin mới nhất
        if (!string.IsNullOrEmpty(currentRoomID))
        {
            selectedRoomID = currentRoomID;
            Room room = RoomStorage.GetRoomByID(selectedRoomID);
            if (room != null)
            {
                UpdateRoomInfo(room);
            }
        }
    }

    void UpdateRoomInfo(Room room)
    {
        List<Vector2> points = room.checkpoints;
        if (points == null || points.Count < 3)
        {
            ClearText();
            return;
        }

        float perimeter = 0f;
        float maxLength = 0f;
        float minLength = float.MaxValue;
        float area = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];
            float dist = Vector2.Distance(a, b);
            perimeter += dist;
            maxLength = Mathf.Max(maxLength, dist);
            minLength = Mathf.Min(minLength, dist);
            area += (a.x * b.y - b.x * a.y); // Shoelace formula
        }

        area = Mathf.Abs(area) * 0.5f;

        lengthText.text = $"Chiều dài: {maxLength:F2} m";
        widthText.text = $"Chiều rộng: {minLength:F2} m";
        perimeterText.text = $"Chu vi: {perimeter:F2} m";
        areaText.text = $"Diện tích: {area:F2} m²";
    }

    void ClearText()
    {
        lengthText.text = "Chiều dài: -";
        widthText.text = "Chiều rộng: -";
        perimeterText.text = "Chu vi: -";
        areaText.text = "Diện tích: -";
    }
}
