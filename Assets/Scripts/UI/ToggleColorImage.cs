using UnityEngine;
using UnityEngine.UI;

public class ToggleColorImage : ToggleColorBase
{
    [SerializeField] private Image toggleColorImage;
    // for prototype
    [SerializeField] private Image toggleSprite;
    [SerializeField] private Sprite deActiveSprite;
    [SerializeField] private Sprite activeSprite;
    public override void Toggle(bool isActive)
    {
        if (toggleColorImage == null)
        {
            Debug.Log("Image is null", gameObject);
            return;
        }
        toggleColorImage.color = isActive ? activeColor : deActiveColor;

        if (!toggleSprite|| !activeSprite || !deActiveSprite)
        {
            return;
        }
        
        toggleSprite.sprite = isActive ? activeSprite : deActiveSprite;
    }

    
}
