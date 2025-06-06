using UnityEngine;

/// <summary>
/// Lớp đại diện cho vật liệu mặt đất trong trò chơi.
/// </summary>
public class OptionCanvas : MonoBehaviour
{
    [HideInInspector] public RectTransform rectTransform;

    private GUICanvasManager guiCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        guiCanvas = GUICanvasManager.instance;

        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
