using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class AreaCalculator
{
    public static GameObject DistanceTextPrefab { get; set; }
    /// <summary>
    /// Tính diện tích ước lượng của đa giác kín dựa trên chu vi.
    /// Công thức: Area = (Perimeter^2) / (4 * PI)
    /// Phù hợp với đa giác đều (ước lượng).
    /// </summary>
    /// <param name="points">Danh sách các điểm tạo đa giác (theo thứ tự).</param>
    /// <returns>Diện tích ước lượng.</returns>
    public static float CalculateArea(List<Vector3> points)
    {

        if (points.Count < 3) 
            return 0f;

        float perimeter = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            perimeter += Vector3.Distance(points[i], points[i + 1]);
        }
        // Nối điểm cuối với điểm đầu
        perimeter += Vector3.Distance(points[points.Count - 1], points[0]);

        float estimatedArea = (perimeter * perimeter) / (4 * Mathf.PI);
        return estimatedArea;
    }
    public static void ShowAreaText(Vector3 position, float area)
    {
        if (DistanceTextPrefab != null)
        {
            GameObject textObj = Object.Instantiate(DistanceTextPrefab, position, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                textMesh.text = $"Dien tich: {area:F2} m2";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogError("distanceTextPrefab khong co TextMeshPro!");
            }
        }
    }
}
