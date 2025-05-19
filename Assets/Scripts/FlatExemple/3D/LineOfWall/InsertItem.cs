using UnityEngine;

public class InsertItem
{
    public LineType type;
    public Vector3 start;
    public Vector3 end;
    public float height;

    public InsertItem(LineType type, Vector3 start, Vector3 end, float height)
    {
        this.type = type;
        this.start = start;
        this.end = end;
        this.height = height;
    }
}
