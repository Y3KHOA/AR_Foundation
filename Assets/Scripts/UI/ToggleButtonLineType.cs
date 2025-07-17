using TMPro;

public class ToggleButtonLineType : ToggleButtonColorUI
{
    public LineType lineType;
    private TextMeshProUGUI textFunc;

    protected override void Awake()
    {
        base.Awake();
        textFunc = GetComponentInChildren<TextMeshProUGUI>();
        string text = null;
        switch (lineType)
        {
            case LineType.Door:
                text = "Cửa";
                break;
            case LineType.Wall:
                text = "Tường";
                break;
            case LineType.Window:
                text = "Cửa sổ";
                break;
        }
        textFunc.text = text;
    }
}