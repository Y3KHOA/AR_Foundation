using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoomInfoCalculator : MonoBehaviour
{
    [Header("TextMeshPro để hiển thị")]
    public TextMeshProUGUI textArea;
    public TextMeshProUGUI textPerimeter;
    public TextMeshProUGUI textHeight;
    public TextMeshProUGUI textVolume;

    [Header("Prefab hiển thị diện tích các tường")]
    public GameObject areaWallPrefab; // prefab chứa 2 TextMeshProUGUI, đã nằm sẵn trong UI để clone

    [Header(" nơi chứa Prefab hiển thị diện tích các tường")]
    public GameObject Body;

    void Start()
    {
        if (RoomStorage.rooms == null || RoomStorage.rooms.Count == 0)
        {
            Debug.LogWarning("No room in RoomStorage.");
            return;
        }

        Room room = RoomStorage.rooms[0];

        List<Vector3> basePoints = new List<Vector3>();
        foreach (var point in room.checkpoints)
        {
            basePoints.Add(new Vector3(point.x, 0f, point.y));
        }

        float area = AreaCalculator.CalculateArea(basePoints);
        float perimeter = PerimeterCalculator.CalculatePerimeter(basePoints);
        float averageHeight = GetAverageHeight(room.heights);
        float volume = VolumeCalculator.CalculateVolume(basePoints, averageHeight);

        Debug.Log($"Room ID: {room.ID}");
        Debug.Log($"Area: {area:F2} m²");
        Debug.Log($"Perimeter: {perimeter:F2} m");
        Debug.Log($"Height Average: {averageHeight:F2} m");
        Debug.Log($"Volume: {volume:F2} m³");

        if (textArea != null) textArea.text = $"{area:F2} m²";
        if (textPerimeter != null) textPerimeter.text = $"{perimeter:F2} m";
        if (textHeight != null) textHeight.text = $"{averageHeight:F2} m";
        if (textVolume != null) textVolume.text = $"{volume:F2} m³";

        // Hiển thị diện tích từng tường
        List<float> wallAreas = AreaWallCalculator.CalculateWallAreas(basePoints, averageHeight);
        for (int i = 0; i < wallAreas.Count; i++)
        {
            // Tạo clone và gán vào cùng parent với prefab gốc
            GameObject wallAreaObj = Instantiate(areaWallPrefab, Body.transform);
            wallAreaObj.SetActive(true); // nếu prefab gốc đang tắt, bật bản clone lên

            TextMeshProUGUI[] texts = wallAreaObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = $"Tường {i + 1}";
                texts[1].text = $"{wallAreas[i]:F2} m²";
            }
            else
            {
                Debug.LogWarning("Prefab thiếu TextMeshProUGUI.");
            }
        }

        // Ẩn prefab gốc đi nếu muốn (chỉ giữ để clone)
        areaWallPrefab.SetActive(false);
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
