using TMPro;
using UnityEngine;

public class InfomationItemCanvas : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI nameItemText;
    public TextMeshProUGUI floorAreaText;

    public void UpdateInfomation(Item item)
    {
        nameItemText.text = item.itemName;
        floorAreaText.text = (item.width * item.length).ToString("F2") + "m²";   
    }    
}
