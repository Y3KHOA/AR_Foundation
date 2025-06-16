using System.Collections.Generic;
using UnityEngine;

// Tính diện tích các mặt tường dựa trên cạnh đáy và chiều cao tường
public static class AreaWallCalculator
{
    /// <summary>
    /// Tính diện tích từng mặt tường.
    /// </summary>
    /// <param name="basePoints">Danh sách điểm đáy (theo thứ tự).</param>
    /// <param name="wallHeight">Chiều cao của tường.</param>
    /// <param name="unit">Đơn vị đo ("cm", "m", "inch", "ft").</param>
    /// <returns>Danh sách diện tích từng mặt tường đã được chuyển đổi sang đơn vị đo.</returns>
    public static List<float> CalculateWallAreas(List<Vector3> basePoints, float wallHeight, string unit = "m")
    {
        List<float> wallAreas = new List<float>();

        if (basePoints.Count < 2 || wallHeight <= 0)
            return wallAreas;

        for (int i = 0; i < basePoints.Count; i++)
        {
            Vector3 currentPoint = basePoints[i];
            Vector3 nextPoint = basePoints[(i + 1) % basePoints.Count]; // Nối điểm cuối về đầu

            float wallLength = Vector3.Distance(currentPoint, nextPoint);
            float wallArea = wallLength * wallHeight;

            // Chuyển đổi đơn vị diện tích
            float convertedArea = ConvertAreaToUnit(wallArea, unit);

            wallAreas.Add(convertedArea);
        }

        return wallAreas;
    }

    /// <summary>
    /// Chuyển đổi diện tích từ mét vuông sang đơn vị đo đã chọn.
    /// </summary>
    private static float ConvertAreaToUnit(float area, string unit)
    {
        switch (unit)
        {
            case "cm": return area * 10000f;  // m² → cm²
            case "inch": return area * 1550f;  // m² → inch²
            case "ft": return area * 10.7639f; // m² → ft²
            case "m":
            default: return area; // Mặc định là mét vuông
        }
    }
}
