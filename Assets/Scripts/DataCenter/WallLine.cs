using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

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
    // Constructor clone
    public WallLine(WallLine other)
    {
        this.start = other.start;
        this.end = other.end;
        this.type = other.type;
        this.distanceHeight = other.distanceHeight ;
        this.Height = other.Height ;
    }
}

/// <summary> 
/// Một phòng hoặc khu vực được xác định bởi đa giác và các đoạn tường tương ứng
/// </summary>
[System.Serializable]
public class Room
{
    public string ID { get; private set; }  // ID chỉ đọc từ bên ngoài

    public List<Vector2> checkpoints = new List<Vector2>(); // polygon chính
    public List<Vector2> extraCheckpoints = new List<Vector2>(); // điểm lẻ trong phòng
    public List<WallLine> wallLines = new List<WallLine>();
    public List<float> heights = new List<float>();

    public Vector2 Compass = new Vector2();
    public float headingCompass; // hướng thực địa của phòng (theo la bàn)

    public Room()
    {
        ID = GenerateID(); // Tự tạo ID khi khởi tạo
    }

    private string GenerateID()
    {
        return Guid.NewGuid().ToString(); // ID ngẫu nhiên toàn cục (UUID)
    }

    public void SetID(string newID)
    {
        ID = newID;
    }

    // Constructor clone
    public Room(Room other)
    {
        ID = other.ID;
        headingCompass = other.headingCompass;
        Compass = other.Compass;

        checkpoints = new List<Vector2>(other.checkpoints);
        wallLines = new List<WallLine>(other.wallLines.Select(w => new WallLine(w))); // clone từng wall
        extraCheckpoints = new List<Vector2>(other.extraCheckpoints);
        heights = new List<float>(other.heights);
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
