using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string timestamp;
    public List<SavedPath> paths = new List<SavedPath>();
}

[Serializable]
public class SavedPath
{
    public List<Vector2Serializable> points;
    public List<float> heights;
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
