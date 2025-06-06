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

    public GameObject panel3DView;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void TurnOn3DView()
    {
        if (panel3DView != null)
        {
            panel3DView.SetActive(true);
        }
        else
        {
            Debug.LogWarning("panel3DView chưa được gán trong Inspector!");
        }
    }
}
