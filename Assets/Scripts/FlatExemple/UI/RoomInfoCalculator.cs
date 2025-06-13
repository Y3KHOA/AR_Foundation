using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoomInfoCalculator : MonoBehaviour
{
    [Header("TextMeshPro để hiển thị")]
    public TextMeshProUGUI textArea;
    public TextMeshProUGUI textCelling;
    public TextMeshProUGUI textHeight;
    public TextMeshProUGUI textVolume;

    void Start()
    {
        if (RoomStorage.rooms == null || RoomStorage.rooms.Count == 0)
        {
            Debug.LogWarning("No room in RoomStorage.");
            return;
        }

        Room room = RoomStorage.rooms[0]; // Lấy phòng đầu tiên

        List<Vector3> basePoints = new List<Vector3>();
        foreach (var point in room.checkpoints)
        {
            basePoints.Add(new Vector3(point.x, 0f, point.y));
        }

        float area = AreaCalculator.CalculateArea(basePoints);
        float celling = CellingCalculator.CalculateCelling(basePoints);
        float averageHeight = GetAverageHeight(room.heights);
        float volume = VolumeCalculator.CalculateVolume(basePoints, averageHeight);

        // Ghi ra debug log
        Debug.Log($"Room ID: {room.ID}");
        Debug.Log($"Area: {area:F2} m²");
        Debug.Log($"Celling: {celling:F2} m");
        Debug.Log($"Height Average: {averageHeight:F2} m");
        Debug.Log($"Volume: {volume:F2} m³");

        // Gán nội dung cho các TextMeshPro tương ứng
        if (textArea != null)
            textArea.text = $"{area:F2} m²";

        if (textCelling != null)
            textCelling.text = $"{celling:F2} m";

        if (textHeight != null)
            textHeight.text = $"{averageHeight:F2} m";

        if (textVolume != null)
            textVolume.text = $"{volume:F2} m³";
    }

    private float GetAverageHeight(List<float> heights)
    {
        if (heights == null || heights.Count == 0)
            return 0f;

        float sum = 0f;
        foreach (float h in heights)
            sum += h;

        return sum / heights.Count;
    }
}
