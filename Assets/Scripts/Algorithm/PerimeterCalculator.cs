using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class PerimeterCalculator
{
    public static GameObject DistanceTextPrefab { get; set; }

    /// <summary>
    /// Tính chu vi của một đa giác khép kín = tổng độ dài các cạnh nối liên tiếp giữa các điểm
    /// </summary>
    public static float CalculatePerimeter(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
            return 0f;

        float perimeter = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            perimeter += Vector3.Distance(points[i], points[i + 1]);
        }

        // Đoạn cuối nối về điểm đầu
        perimeter += Vector3.Distance(points[points.Count - 1], points[0]);

        return perimeter;
    }

    /// <summary>
    /// Hiển thị chu vi đã tính dưới dạng text tại vị trí chỉ định.
    /// </summary>
    public static void ShowPerimeterText(Vector3 position, float rawPerimeter)
    {
        string unit = PlayerPrefs.GetString("SelectedUnit", "m");
        float converted = ConvertPerimeterToUnit(rawPerimeter, unit);

        if (DistanceTextPrefab != null)
        {
            GameObject textObj = Object.Instantiate(DistanceTextPrefab, position, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                textMesh.text = $"Chu vi: {converted:F2} {unit}";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogWarning("DistanceTextPrefab không có TextMeshPro component.");
            }
        }
    }

    /// <summary>
    /// Chuyển đổi chu vi từ mét sang đơn vị được chọn (cm, inch, ft).
    /// </summary>
    private static float ConvertPerimeterToUnit(float perimeter, string unit)
    {
        switch (unit)
        {
            case "cm": return perimeter * 100f;
            case "inch": return perimeter * 39.3701f;
            case "ft": return perimeter * 3.28084f;
            case "m":
            default: return perimeter;
        }
    }
}
