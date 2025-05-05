using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Một đoạn đường thẳng đại diện cho tường, cửa hoặc cửa sổ
/// </summary>
[System.Serializable]
public class WallLine
{
    public Vector3 start;
    public Vector3 end;
    public LineType type; // Wall, Door, Window

    public WallLine(Vector3 start, Vector3 end, LineType type)
    {
        this.start = start;
        this.end = end;
        this.type = type;
    }
}

/// <summary>
/// Một phòng hoặc khu vực được xác định bởi đa giác và các đoạn tường tương ứng
/// </summary>
[System.Serializable]
public class Room
{
    public List<Vector2> checkpoints = new List<Vector2>();
    public List<WallLine> wallLines = new List<WallLine>();

    /// <summary>
    /// Sinh các đoạn tường từ polygon
    /// </summary>
    private List<WallLine> GenerateWallsFromPolygon(List<Vector2> polygon)
    {
        List<WallLine> result = new List<WallLine>();
        if (polygon == null || polygon.Count < 2) return result;

        bool closed = Vector2.Distance(polygon[0], polygon[^1]) < 0.01f;
        int count = closed ? polygon.Count - 1 : polygon.Count;

        for (int i = 0; i < count; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Count];

            Vector3 start = new Vector3(p1.x, 0, p1.y);
            Vector3 end = new Vector3(p2.x, 0, p2.y);

            result.Add(new WallLine(start, end, LineType.Wall));
        }

        return result;
    }
}

/// <summary>
/// Loại đường thẳng đại diện cho tường, cửa, hoặc cửa sổ
/// </summary>
public enum LineType
{
    Wall,
    Door,
    Window
}
