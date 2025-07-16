using UnityEngine;
using UnityEngine.UI;

public class ToggleColor : MonoBehaviour
{
    [SerializeField] private Color deActiveColor;
    [SerializeField] private Color activeColor;
    [SerializeField] private Image toggleColorImage;

    public void Toggle(bool isActive)
    {
        toggleColorImage.color = isActive ? activeColor : deActiveColor;
    }
}
