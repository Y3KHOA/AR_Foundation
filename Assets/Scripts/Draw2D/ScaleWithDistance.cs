using UnityEngine;
using TMPro;

public class ScaleWithDistance : MonoBehaviour
{
    public float baseFontSize = 2.5f;
    public float fontSizeMultiplier = 0.05f;

    private TextMeshPro tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        if (tmp == null || Camera.main == null) return;

        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        tmp.fontSize = baseFontSize * distance * fontSizeMultiplier;
    }
}
