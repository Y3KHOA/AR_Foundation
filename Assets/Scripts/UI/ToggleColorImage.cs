using UnityEngine;
using UnityEngine.UI;

public class ToggleColorImage : ToggleColorBase
{
    [SerializeField] private Image toggleColorImage;

    public override void Toggle(bool isActive)
    {
        if (toggleColorImage == null)
        {
            Debug.Log("Image is null", gameObject);
            return;
        }
        toggleColorImage.color = isActive ? activeColor : deActiveColor;
    }

    
}