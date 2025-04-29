using UnityEngine;
/// <summary>
/// nơi lưu dữ liệu tường - cửa - cửa sổ
/// </summary>
[System.Serializable]
public class WallLine
{
    public Vector3 start;
    public Vector3 end;
    public LineType type; // wall, door, window...

    public WallLine(Vector3 start, Vector3 end, LineType type)
    {
        this.start = start;
        this.end = end;
        this.type = type;
    }
}

public enum LineType
{
    Wall,
    Door,
    Window
}
