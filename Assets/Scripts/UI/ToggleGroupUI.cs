using System.Collections.Generic;
using UnityEngine;

public class ToggleGroupUI : MonoBehaviour
{
    [SerializeField] private List<ToggleButtonLineType> list = new();
    public DrawingTool drawingTool;
    public CheckpointManager checkpointManager;
    public PenManager penManager;
    
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
        
    }

    public void ShowFirstButton()
    {
        OnSelectThis(list[0]);
    }

    public void ToggleOffAll()
    {
        foreach (var item in list)
        {
            item.ChangeState(ToggleButtonUIBase.State.DeActive);
        }

    }

    private void OnSelectThis(ToggleButtonLineType btn)
    {
        if (btn.currentState == ToggleButtonUIBase.State.DeActive)
        {
            btn.ChangeState(ToggleButtonUIBase.State.Active);
            // lock pen
            penManager.ChangeState(false);
        }
        else
        {
            // unlock pen, do chỉ 1 btn được bật nên không cần chạy code ở dưới
            btn.ChangeState(ToggleButtonUIBase.State.DeActive);
            penManager.ChangeState(true);
            return;
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