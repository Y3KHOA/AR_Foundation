using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này xử lý giao diện người dùng bật lên trong trò chơi, cho phép hiển thị tin nhắn hoặc thông báo.
/// </summary>
public class OptionConfig : MonoBehaviour
{
    public Button onBTN;
    public Button offBTN;
    public GameObject canvas;
    public bool isOpen = false;

    private void Start()
    {
        if (!isOpen)
        {
            onBTN.gameObject.SetActive(false);
            offBTN.gameObject.SetActive(true);
        }
    }
}
