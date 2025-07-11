using UnityEngine;
// using UISwitcher; // namespace

public class LineTypeToggleManager : MonoBehaviour
{
    public UISwitcher.UISwitcher wallSwitch;
    public UISwitcher.UISwitcher doorSwitch;
    public UISwitcher.UISwitcher windowSwitch;

    public DrawingTool drawingTool;
    public CheckpointManager checkpointManager;

    private void Start()
    {
        wallSwitch.onValueChanged.AddListener((isOn) => OnSwitchChanged(wallSwitch, isOn, LineType.Wall));
        doorSwitch.onValueChanged.AddListener((isOn) => OnSwitchChanged(doorSwitch, isOn, LineType.Door));
        windowSwitch.onValueChanged.AddListener((isOn) => OnSwitchChanged(windowSwitch, isOn, LineType.Window));

        wallSwitch.isOn = true; // Bật mặc định
    }

    void OnSwitchChanged(UISwitcher.UISwitcher changedSwitch, bool isOn, LineType type)
    {
        if (!isOn) return;
        // Tắt các switch còn lại
        if (changedSwitch != wallSwitch) wallSwitch.isOn = false;
        if (changedSwitch != doorSwitch) doorSwitch.isOn = false;
        if (changedSwitch != windowSwitch) windowSwitch.isOn = false;

        // Gán loại line hiện tại
        if (drawingTool != null)
        {
            drawingTool.currentLineType = type;
        }
        if (checkpointManager != null)
        {
            checkpointManager.currentLineType = type;
        }
    }
}
