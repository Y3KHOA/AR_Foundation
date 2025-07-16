using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AutoResize : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    [SerializeField] private float matchValue1 = 0;
    [SerializeField] private float matchValue2 = 1;

    private void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
    }

    private void Update()
    {
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
            return;
        }

        aspect = (float)Screen.height / Screen.width;
        Debug.Log($"Width: {Screen.height} Height {Screen.width}");
        // canvasScaler.matchWidthOrHeight = Screen.width > Screen.height ? test1 : test2;
        
        if (aspect > 1.45f)
        {
            // Widescreen → ưu tiên chiều cao
            canvasScaler.matchWidthOrHeight = matchValue1;
        }
        else
        {
            // Gần vuông (iPad, 4:3, 6:5) → ưu tiên chiều rộng
            canvasScaler.matchWidthOrHeight = matchValue2;
        }
    }

    [SerializeField] private float aspect;
}