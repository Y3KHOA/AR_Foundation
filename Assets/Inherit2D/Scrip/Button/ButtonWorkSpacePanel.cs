using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Lớp này quản lý bảng điều khiển không gian làm việc để chuyển đổi giữa chế độ xem 2D và 3D trong trò chơi.
/// </summary>
public class ButtonWorkSpacePanel : MonoBehaviour
{
    private GameManager gameManager;

    [Header("3D Button")]
    public Sprite sprite3D;
    public Sprite sprite2D;
    public Button button3D;

    [Header("Canvas")]
    public List<GameObject> canvasList;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void TurnOn3DView()
    {
        float defaultWallHeight = 1.0f;

        // Giả sử bạn đang thao tác trên Room đầu tiên, hoặc có biến `currentRoom`
        foreach (Room room in RoomStorage.rooms)
        {
            foreach (WallLine wall in room.wallLines)
            {
                if (wall.type == LineType.Wall)
                {
                    wall.distanceHeight = 0f;         // bắt đầu từ mặt đất
                    wall.Height = defaultWallHeight;  // chiều cao tường
                }
                // Door và Window giữ nguyên height/distanceHeight
            }

            // Optional: Nếu muốn, bạn cũng có thể clear danh sách `heights` cũ và add lại
            room.heights.Clear();
            for (int i = 0; i < room.wallLines.Count; i++)
            {
                room.heights.Add(room.wallLines[i].Height);
            }
        }

        // Chuyển scene
        SceneManager.LoadScene("FlatExampleScene");
    }
}
