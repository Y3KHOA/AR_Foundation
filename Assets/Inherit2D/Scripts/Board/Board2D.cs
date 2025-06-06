using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lớp này tạo ra một lưới động trong không gian 2D, cho phép các ô vuông được tạo ra và di chuyển theo vị trí của camera.
/// </summary>
public class DynamicGrid : MonoBehaviour
{
    public GameObject boxPrefab;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;

    private Transform[,] grid;
    private Camera mainCamera;
    private Vector2Int currentOrigin;

    void Start()
    {
        mainCamera = Camera.main;
        grid = new Transform[gridWidth, gridHeight];

        // Khởi tạo lưới
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject box = Instantiate(boxPrefab, transform);
                grid[x, y] = box.transform;
            }
        }

        UpdateGrid();
    }

    void Update()
    {
        Vector2Int newOrigin = GetCameraCenterCell();

        if (newOrigin != currentOrigin)
        {
            currentOrigin = newOrigin;
            UpdateGrid();
        }
    }

    Vector2Int GetCameraCenterCell()
    {
        Vector3 camPos = mainCamera.transform.position;
        int cx = Mathf.FloorToInt(camPos.x / cellSize);
        int cy = Mathf.FloorToInt(camPos.y / cellSize);
        return new Vector2Int(cx, cy);
    }

    void UpdateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int worldX = currentOrigin.x + x - gridWidth / 2;
                int worldY = currentOrigin.y + y - gridHeight / 2;
                grid[x, y].position = new Vector3(worldX * cellSize, worldY * cellSize, 0);
            }
        }
    }
}
