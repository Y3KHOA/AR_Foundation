using TMPro;
using UnityEngine;

/// <summary>
/// Lớp này quản lý việc hiển thị thông tin cho các vật phẩm trong trò chơi.
/// </summary>
public class InfomationItemCanvas : MonoBehaviour
{
    public static InfomationItemCanvas instance;

    public TextMeshProUGUI nameItemText;
    public TextMeshProUGUI floorAreaText;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateInfomation(Item item)
    {
        nameItemText.text = item.itemName;
        floorAreaText.text = (item.width * item.length).ToString("F2") + "m²";
    }
}
