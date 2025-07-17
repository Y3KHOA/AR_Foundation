using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    public float cellSize = 0.5f;
    public float viewRange = 10f; // Phạm vi hiển thị lưới quanh camera
    private Camera cam;
    public Material backgroundMaterial; // Gán trong Inspector
    private GameObject background;
    private Dictionary<Vector3Int, GameObject> gridLines = new();
    public Material test;

    private const int HorizontalID = 0;
    private const int VerticalID = 1;
    private HashSet<Vector3Int> visibleLines = new();

    private Stack<LineRenderer> stacks = new();
    private int maxItemCount = 600;

    void Start()
    {
        cam = Camera.main;

        background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "Background";
        background.GetComponent<Renderer>().material = backgroundMaterial;
        background.layer = LayerMask.NameToLayer("Background");

        // XZ plane nên quay Quad nằm phẳng XZ
        background.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Init();
    }

    void Update()
    {
        UpdateGridAroundCamera();

        float width = viewRange * 2;
        float height = viewRange * 2;
        background.transform.position =
            new Vector3(cam.transform.position.x, -5f, cam.transform.position.z); // Y thấp hơn để nằm dưới lưới
        background.transform.localScale = new Vector3(width, height, 1);
    }

    void UpdateGridAroundCamera()
    {
        if (cam == null || !cam.orthographic) return;

        Vector3 camPos = cam.transform.position;
        int minX = Mathf.FloorToInt((camPos.x - viewRange) / cellSize);
        int maxX = Mathf.CeilToInt((camPos.x + viewRange) / cellSize);
        int minZ = Mathf.FloorToInt((camPos.z - viewRange) / cellSize);
        int maxZ = Mathf.CeilToInt((camPos.z + viewRange) / cellSize);

        visibleLines.Clear();

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Hàng ngang (trục X)
                Vector3Int key = new Vector3Int(HorizontalID, x, z);
                visibleLines.Add(key);
                if (!gridLines.ContainsKey(key))
                {
                    Vector3 start = new Vector3(x * cellSize, 0, z * cellSize);
                    Vector3 end = new Vector3((x + 1) * cellSize, 0, z * cellSize);
                    GameObject line = CreateLine(start, end);
                    gridLines[key] = line;
                }

                // Hàng dọc (trục Z)
                key = new Vector3Int(VerticalID, x, z);
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

        // Xoá line không còn hiển thị
        var keys = new List<Vector3Int>(gridLines.Keys);
        foreach (var key in keys)
        {
            if (!visibleLines.Contains(key))
            {
                ReturnItem(gridLines[key].GetComponent<LineRenderer>());
                gridLines.Remove(key);
            }
        }
    }

    void Init()
    {
        for (int i = 0; i < maxItemCount; i++)
        {
            var lr = Create();
            stacks.Push(lr);
            lr.gameObject.SetActive(false);
        }
    }

    LineRenderer Get()
    {
        LineRenderer item = null;
        if (stacks.Count > 0)
        {
            item = stacks.Pop();
        }
        else
        {
            item = Create();
        }

        item.gameObject.SetActive(true);
        return item;
    }

    void ReturnItem(LineRenderer lr)
    {
        if (stacks.Count > maxItemCount)
        {
            Destroy(lr.gameObject);
        }
        else
        {
            lr.gameObject.SetActive(false);
            stacks.Push(lr);
        }
    }

    LineRenderer Create()
    {
        GameObject line = new GameObject("GridLine");
        LineRenderer lr = line.AddComponent<LineRenderer>();

        lr.sortingOrder = -1000;

        lr.positionCount = 2;
        lr.material = test;
        lr.useWorldSpace = true;

        lr.startWidth = lr.endWidth = 0.02f;
        lr.startColor = lr.endColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        return lr;
    }

    GameObject CreateLine(Vector3 start, Vector3 end)
    {
        var lr = Get();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = test;
        lr.useWorldSpace = true;

        lr.startWidth = lr.endWidth = 0.02f;
        lr.startColor = lr.endColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        return lr.gameObject;
    }
}
