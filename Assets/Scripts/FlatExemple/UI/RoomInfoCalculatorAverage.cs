using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoomInfoCalculatorAverage : MonoBehaviour
{
    [Header("TextMeshPro để hiển thị trung bình")]
    public TextMeshProUGUI textArea;
    public TextMeshProUGUI textCelling;
    public TextMeshProUGUI textHeight;
    public TextMeshProUGUI textVolume;

    void Start()
    {
        List<Room> rooms = RoomStorage.rooms;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No room in RoomStorage.");
            return;
        }

        float totalArea = 0f;
        float totalCelling = 0f;
        float totalHeight = 0f;
        float totalVolume = 0f;

        int roomCount = rooms.Count;

        foreach (Room room in rooms)
        {
            List<Vector3> basePoints = new List<Vector3>();
            foreach (var point in room.checkpoints)
            {
                basePoints.Add(new Vector3(point.x, 0f, point.y));
            }

            float area = AreaCalculator.CalculateArea(basePoints);
            float celling = CellingCalculator.CalculateCelling(basePoints);
            float height = GetAverageHeight(room.heights);
            float volume = VolumeCalculator.CalculateVolume(basePoints, height);

            totalArea += area;
            totalCelling += celling;
            totalHeight += height;
            totalVolume += volume;
        }

        float avgArea = totalArea / roomCount;
        float avgCelling = totalCelling / roomCount;
        float avgHeight = totalHeight / roomCount;
        float avgVolume = totalVolume / roomCount;

        // Hiển thị
        if (textArea != null)
            textArea.text = $"{avgArea:F2} m²";
        if (textCelling != null)
            textCelling.text = $"{avgCelling:F2} m";
        if (textHeight != null)
            textHeight.text = $"{avgHeight:F2} m";
        if (textVolume != null)
            textVolume.text = $"{avgVolume:F2} m³";

        Debug.Log($"== TRUNG BÌNH {roomCount} PHÒNG ==");
        Debug.Log($"Area: {avgArea:F2} m²");
        Debug.Log($"Celling: {avgCelling:F2} m");
        Debug.Log($"Height: {avgHeight:F2} m");
        Debug.Log($"Volume: {avgVolume:F2} m³");
    }

    private float GetAverageHeight(List<float> heights)
    {
        if (heights == null || heights.Count == 0) return 0f;

        float sum = 0f;
        foreach (float h in heights)
            sum += h;

        return sum / heights.Count;
    }
}
