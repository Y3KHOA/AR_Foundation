using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    North,
    South,
    East,
    West
}

public enum AnchorPosition
{
    MiddleTop,
    MiddleBottom,
    MiddleLeft,
    MiddleRight,
    Center
}
public static class AnchorExtensions
{
    public static (Vector2 min, Vector2 max) ToAnchorPreset(this AnchorPosition pos)
    {
        return pos switch
        {
            AnchorPosition.MiddleTop    => (new Vector2(0.5f, 1f), new Vector2(0.5f, 1f)),
            AnchorPosition.MiddleBottom => (new Vector2(0.5f, 0f), new Vector2(0.5f, 0f)),
            AnchorPosition.MiddleLeft   => (new Vector2(0f, 0.5f), new Vector2(0f, 0.5f)),
            AnchorPosition.MiddleRight  => (new Vector2(1f, 0.5f), new Vector2(1f, 0.5f)),
            AnchorPosition.Center       => (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
            _ => (Vector2.zero, Vector2.zero)
        };
    }
    public static Vector2 GetOffset(this AnchorPosition pos, Vector2 size)
    {
        return pos switch
        {
            AnchorPosition.MiddleTop    => new Vector2(0f, -size.y / 2f),
            AnchorPosition.MiddleBottom => new Vector2(0f, size.y / 2f),
            AnchorPosition.MiddleLeft   => new Vector2(size.x / 2f, 0f),
            AnchorPosition.MiddleRight  => new Vector2(-size.x / 2f, 0f),
            AnchorPosition.Center       => Vector2.zero,
            _ => Vector2.zero
        };
    }
    
}
public static class DirectionAnchorMapping
{
    public static AnchorPosition ToAnchor(this Direction dir)
    {
        return dir switch
        {
            Direction.North => AnchorPosition.MiddleBottom,
            Direction.South => AnchorPosition.MiddleTop,
            Direction.East  => AnchorPosition.MiddleRight,
            Direction.West  => AnchorPosition.MiddleLeft,
            _ => AnchorPosition.Center
        };
    }
}

public class DirectionRotationCalculator
{
    public List<Direction> circleDirection = new();
    private Dictionary<Direction, float> directionsOffet = new();
    public float offset = 90;

    public void Init()
    {
        float defaultValue = 0;
        directionsOffet.Clear();
        for (int i = 0; i < circleDirection.Count; i++)
        {
            directionsOffet.Add(circleDirection[i],defaultValue);
            defaultValue += offset;
        }

        foreach (var item in directionsOffet)
        {
            Debug.Log($"Item {item.Key} {item.Value}");
        }
    }

    public void SetZRotation(RectTransform rectTransform, Direction rectDirection)
    {
        rectTransform.transform.rotation = Quaternion.Euler(0,0,directionsOffet[rectDirection]);
    }
}


public class DirectionalItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;
    private Direction direction;
    
    public RectTransform Icon;
    
    public void Set(Direction direction)
    {
        this.direction = direction;
        Text.text = GetDirectionName(direction);
    }
    
    public void SetAnchor(RectTransform rt, AnchorPosition anchor)
    {
        var (min, max) = anchor.ToAnchorPreset();
        rt.anchorMin = min;
        rt.anchorMax = max;
        
        Vector2 offset = anchor.GetOffset(rt.sizeDelta);
        rt.anchoredPosition = offset;
    }
    
    public static string GetDirectionName(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return "Bắc";
            case Direction.South: return "Nam";
            case Direction.East:  return "Đông";
            case Direction.West:  return "Tây";
            default: return "Không rõ";
        }
    }

}
