using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    public float cellSize = 0.5f;
    public float viewRange = 10f; // Phạm vi hiển thị lưới quanh camera
    private Camera cam;

    // private Dictionary<Vector2Int, GameObject> gridLines = new();
    private Dictionary<string, GameObject> gridLines = new();

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        UpdateGridAroundCamera();
    }

    void UpdateGridAroundCamera()
    {
        if (cam == null || !cam.orthographic) return;

        Vector3 camPos = cam.transform.position;
        int minX = Mathf.FloorToInt((camPos.x - viewRange) / cellSize);
        int maxX = Mathf.CeilToInt((camPos.x + viewRange) / cellSize);
        int minZ = Mathf.FloorToInt((camPos.z - viewRange) / cellSize);
        int maxZ = Mathf.CeilToInt((camPos.z + viewRange) / cellSize);

        HashSet<string> visibleLines = new();

        // Vẽ hàng ngang
        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++) // sửa từ < maxX thành <= maxX
            {
                string key = $"H_{x}_{z}";
                visibleLines.Add(key);
                if (!gridLines.ContainsKey(key))
                {
                    Vector3 start = new Vector3(x * cellSize, 0, z * cellSize);
                    Vector3 end = new Vector3((x + 1) * cellSize, 0, z * cellSize);
                    GameObject line = CreateLine(start, end);
                    gridLines[key] = line;
                }
            }
        }

        // Vẽ hàng dọc
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++) // sửa từ < maxZ thành <= maxZ
            {
                string key = $"V_{x}_{z}";
                visibleLines.Add(key);
                if (!gridLines.ContainsKey(key))
                {
                    Vector3 start = new Vector3(x * cellSize, 0, z * cellSize);
                    Vector3 end = new Vector3(x * cellSize, 0, (z + 1) * cellSize);
                    GameObject line = CreateLine(start, end);
                    gridLines[key] = line;
                }
            }
        }

        // Xóa các line không còn nằm trong vùng hiển thị
        var keys = new List<string>(gridLines.Keys);
        foreach (var key in keys)
        {
            if (!visibleLines.Contains(key))
            {
                Destroy(gridLines[key]);
                gridLines.Remove(key);
            }
        }
    }

    GameObject CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = 0.02f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        return line;
    }
}
