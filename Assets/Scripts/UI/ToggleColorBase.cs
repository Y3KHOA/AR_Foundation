using UnityEngine;

public abstract class ToggleColorBase : MonoBehaviour
{
    [SerializeField] protected ToggleColorPreset preset;
    [SerializeField] protected bool isUsingPreset;
    [SerializeField] protected Color deActiveColor;
    [SerializeField] protected Color activeColor;
    
    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        if (isUsingPreset && preset)
        {
            deActiveColor = preset.deActiveColor;
            activeColor = preset.activeColor;
        }
#endif
    }
    public abstract void Toggle(bool isActive);
}