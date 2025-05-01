using UnityEngine;

public class SymbolData
{
    public Vector2 center;
    public float angleDeg;
    public string type; // "door" hoặc "window"
    public float width;

    public SymbolData(Vector2 center, float angleDeg, string type, float width)
    {
        this.center = center;
        this.angleDeg = angleDeg;
        this.type = type;
        this.width = width;
    }
}
