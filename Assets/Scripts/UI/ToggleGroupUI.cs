using System.Collections.Generic;
using UnityEngine;

public class ToggleGroupUI : MonoBehaviour
{
    [SerializeField] private List<ToggleButtonLineType> list = new();
    public DrawingTool drawingTool;
    public CheckpointManager checkpointManager;
    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        foreach (ToggleButtonLineType item in list)
        {
            item.btn.onClick.AddListener(() =>
            {
                OnSelectThis(item);
            });   
        }
        OnSelectThis(list[0]);
    }

    private void OnSelectThis(ToggleButtonLineType btn)
    {
        if (btn.currentState == ToggleButtonUIBase.State.DeActive)
        {
            btn.ChangeState(ToggleButtonUIBase.State.Active);
        }

        foreach (var item in list)
        {
            if (item != btn)
            {
                item.ChangeState(ToggleButtonUIBase.State.DeActive);
            }
        }

        drawingTool.currentLineType = btn.lineType;
        checkpointManager.currentLineType = btn.lineType;
    }
}