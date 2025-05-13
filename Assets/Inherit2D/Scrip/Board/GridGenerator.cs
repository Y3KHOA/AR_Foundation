using UnityEngine;
using System.Collections.Generic;

public class GridGenerator2 : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject boxPrefab;
    public float cellSize = 1f;
    public float viewRange = 10f;

    private Camera cam;
    private Dictionary<Vector2Int, GameObject> gridBoxes = new Dictionary<Vector2Int, GameObject>();

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
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;

        // Xác định giới hạn hiển thị dựa trên viewRange
        int minX = Mathf.FloorToInt((camPos.x - viewRange) / cellSize);
        int maxX = Mathf.CeilToInt((camPos.x + viewRange) / cellSize);
        int minZ = Mathf.FloorToInt((camPos.z - viewRange) / cellSize);
        int maxZ = Mathf.CeilToInt((camPos.z + viewRange) / cellSize);

        HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector2Int cellPos = new Vector2Int(x, z);
                visibleCells.Add(cellPos);

                if (!gridBoxes.ContainsKey(cellPos))
                {
                    Vector3 boxPos = new Vector3(x * cellSize, 0, z * cellSize);
                    GameObject box = Instantiate(boxPrefab, boxPos, Quaternion.identity, transform);

                    // Tối ưu hiển thị nếu cần
                    AdjustBoxAppearance(box, cellPos);

                    gridBoxes.Add(cellPos, box);
                }
            }
        }

        // Xoá box không còn trong phạm vi hiển thị
        RemoveInvisibleBoxes(visibleCells);
    }

    void AdjustBoxAppearance(GameObject box, Vector2Int cellPos)
    {
        // Điều chỉnh kích thước hoặc màu của box nếu muốn tạo ô đậm/nhạt.
        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Đánh dấu mỗi ô thứ 4 bằng màu/scale đậm hơn.
            if (cellPos.x % 4 == 0 || cellPos.y % 4 == 0)
            {
                renderer.material.color = new Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                renderer.material.color = new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }

    void RemoveInvisibleBoxes(HashSet<Vector2Int> visibleCells)
    {
        List<Vector2Int> keysToRemove = new List<Vector2Int>();

        foreach (var cell in gridBoxes.Keys)
        {
            if (!visibleCells.Contains(cell))
            {
                Destroy(gridBoxes[cell]);
                keysToRemove.Add(cell);
            }
        }

        foreach (var key in keysToRemove)
        {
            gridBoxes.Remove(key);
        }
    }
}
