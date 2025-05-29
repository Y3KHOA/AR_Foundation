using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Board3D : MonoBehaviour
{
    public GameObject boxPrefab;
    public int rows = 20;
    public int cols = 30;
    private float cellSize = 10f;
    private Vector3 firstBoxPosition;

    void Start()
    {
        firstBoxPosition = transform.position;
        CreateBoard();
    }

    void CreateBoard()
    {
        //7.5: mỗi ô có chiều dài và chiều rộng = 7.5
        float x = firstBoxPosition.x, y = firstBoxPosition.y + cellSize;
        for (int row = 0; row < rows; row++)
        {
            y -= cellSize;
            for (int column = 0; column < cols; column++)
            {          
                // Tính toán vị trí của ô vuông.
                Vector3 position = new Vector3(x, y, 0);
                // Tạo ô vuông tại vị trí đã tính toán.
                Instantiate(boxPrefab, position, Quaternion.identity, transform);
                x += cellSize;
            }
            x = firstBoxPosition.x;
        }
    }
}
