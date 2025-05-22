using TMPro;
using UnityEngine;

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
