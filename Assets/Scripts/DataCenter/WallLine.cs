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
    public WallLine() { }

    // Dùng cho cửa sổ:
    public float distanceHeight = 0f;   // độ cao bắt đầu từ mặt đất
    public float Height = 0f; // chiều cao của cửa / cửa sổ (độ dày theo trục Y)

    public WallLine(Vector3 start, Vector3 end, LineType type, float baseHeight = 0f, float windowHeight = 0f)
    {
        this.start = start;
        this.end = end;
        this.type = type;
        this.distanceHeight = baseHeight;
        this.Height = windowHeight;
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
    public List<float> heights = new List<float>();

    public Vector2 Compass= new Vector2();
    public float headingCompass; // hướng thực địa của phòng (theo la bàn)

    public float area;
    public float ceilingArea;
    public float perimeter;
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
