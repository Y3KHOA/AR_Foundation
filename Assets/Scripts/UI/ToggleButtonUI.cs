using System;
using UnityEngine;
using UnityEngine.Events;

public class ToggleButtonUI : ToggleButtonColorUI
{
    public Action ActiveEvent;
    protected override void Awake()
    {
        base.Awake();
        ChangeState(currentState);
    }
    
    public void OnActive()
    {
        ActiveEvent?.Invoke();
    }
}