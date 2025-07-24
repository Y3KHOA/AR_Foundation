using TMPro;
using UnityEngine;

public class ToggleColorTMPro : ToggleColorBase
{
    [SerializeField] private TextMeshProUGUI tmpPro;


    public override void Toggle(bool isActive)
    {
        if (tmpPro == null)
        {
            Debug.Log("TMPro is null", gameObject);
            return;
        }
        tmpPro.color = isActive ? activeColor : deActiveColor;
    }
}