using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Tính thể tích = Diện tích đáy × Chiều cao
/// </summary>
public static class VolumeCalculator
{
    public static GameObject DistanceTextPrefab { get; set; }

    /// <summary>
    /// Tính thể tích của khối đa giác có chiều cao cho trước.
    /// </summary>
    /// <param name="points">Danh sách điểm đáy (đa giác khép kín).</param>
    /// <param name="height">Chiều cao khối.</param>
    /// <returns>Thể tích theo mét khối.</returns>
    public static float CalculateVolume(List<Vector3> points, float height)
    {
        if (points == null || points.Count < 3 || height <= 0f)
            return 0f;

        float area = AreaCalculator.CalculateArea(points); // Tính diện tích đáy
        float volume = area * height; // Thể tích = diện tích × chiều cao
        return volume;
    }

    /// <summary>
    /// Hiển thị văn bản thể tích tại vị trí chỉ định.
    /// </summary>
    public static void ShowVolumeText(Vector3 position, float volume)
    {
        string unit = PlayerPrefs.GetString("SelectedUnit", "m");
        float converted = ConvertVolumeToUnit(volume, unit);

        if (DistanceTextPrefab != null)
        {
            GameObject textObj = Object.Instantiate(DistanceTextPrefab, position, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                textMesh.text = $"Thể tích: {converted:F2} {unit}³";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogWarning("DistanceTextPrefab không chứa TextMeshPro.");
            }
        }
    }

    /// <summary>
    /// Chuyển đổi đơn vị thể tích từ m³ sang đơn vị mong muốn.
    /// </summary>
    private static float ConvertVolumeToUnit(float volume, string unit)
    {
        switch (unit)
        {
            case "cm": return volume * 1_000_000f;   // m³ → cm³
            case "inch": return volume * 61023.7f;   // m³ → inch³
            case "ft": return volume * 35.3147f;     // m³ → ft³
            default: return volume;                  // Mặc định: m³
        }
    }
}
