using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

/// <summary>
/// Lớp này tạo và quản lý lưới hiển thị quanh camera trong chế độ 2D.
/// </summary>
public class BoardTest : MonoBehaviour
{
    public float cellSize = 0.5f;
    public float viewRange = 10f; // Phạm vi hiển thị lưới quanh camera
    private Camera cam;
    public Material backgroundMaterial; // Gán trong Inspector
    private GameObject background;
    private Dictionary<string, GameObject> gridLines = new();
    public Material test;

    void Start()
    {
        cam = Camera.main;

        background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "Background";
        background.GetComponent<Renderer>().material = backgroundMaterial;
        background.layer = LayerMask.NameToLayer("Background"); // optional
    }

    void Update()
    {
        UpdateGridAroundCamera();

        float width = viewRange * 2;
        float height = viewRange * 2;
        background.transform.position =
            new Vector3(cam.transform.position.x, cam.transform.position.y, 1f); // Z = 1 để nằm sau lưới
        background.transform.localScale = new Vector3(width, height, 1);
    }

    void UpdateGridAroundCamera()
    {
        if (cam == null || !cam.orthographic) return;

        Vector3 camPos = cam.transform.position;
        int minX = Mathf.FloorToInt((camPos.x - viewRange) / cellSize);
        int maxX = Mathf.CeilToInt((camPos.x + viewRange) / cellSize);
        int minY = Mathf.FloorToInt((camPos.y - viewRange) / cellSize);
        int maxY = Mathf.CeilToInt((camPos.y + viewRange) / cellSize);

        HashSet<string> visibleLines = new();

        // Vẽ hàng ngang
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                string key = $"H_{x}_{y}";
                visibleLines.Add(key);
                if (!gridLines.ContainsKey(key))
                {
                    Vector3 start = new Vector3(x * cellSize, y * cellSize, 0);
                    Vector3 end = new Vector3((x + 1) * cellSize, y * cellSize, 0);
                    // bool isBold = (y % 4 == 0);
                    // GameObject line = CreateLine(start, end, isBold);
                    GameObject line = CreateLine(start, end);
                    gridLines[key] = line;
                }
            }
        }

        // Vẽ hàng dọc
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                string key = $"V_{x}_{y}";
                visibleLines.Add(key);
                if (!gridLines.ContainsKey(key))
                {
                    Vector3 start = new Vector3(x * cellSize, y * cellSize, 0);
                    Vector3 end = new Vector3(x * cellSize, (y + 1) * cellSize, 0);
                    // bool isBold = (x % 4 == 0);
                    // GameObject line = CreateLine(start, end, isBold);
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
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = test;
        // lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;

        // float thickness = cam.orthographicSize / 200f;
        // if (isBold)
        // {
        //     lr.startWidth = lr.endWidth = 0.04f;
        //     lr.startColor = lr.endColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        // }
        // else
        // {
        //     lr.startWidth = lr.endWidth = 0.02f;
        //     lr.startColor = lr.endColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        // }

        lr.startWidth = lr.endWidth = 0.02f;
        lr.startColor = lr.endColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        return line;
    }
}