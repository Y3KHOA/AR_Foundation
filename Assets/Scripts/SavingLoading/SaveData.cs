using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string timestamp;
    public float area;
    public float perimeter;
    public float ceiling;
    public List<SavedPath> paths = new List<SavedPath>();
}

[Serializable]
public class SavedPath
{
    public List<Vector2Serializable> points;
    public List<float> heights;

    public float area;       // diện tích mặt đáy
    public float perimeter;  // chu vi (tổng chiều dài các cạnh)
    public float ceiling;  // diện tích mặt trần
}

[Serializable]
public class Vector2Serializable
{
    public float x, y;

    public Vector2Serializable(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
}
