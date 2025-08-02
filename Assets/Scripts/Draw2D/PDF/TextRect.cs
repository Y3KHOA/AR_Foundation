using UnityEngine;

public struct TextRect
{
    public float x, y, width, height;

    public TextRect(float x, float y, float width, float height)
    {
        this.x = x; this.y = y;
        this.width = width; this.height = height;
    }

    public bool Intersects(TextRect other)
    {
        return !(other.x > x + width ||
                 other.x + other.width < x ||
                 other.y > y + height ||
                 other.y + other.height < y);
    }
}
