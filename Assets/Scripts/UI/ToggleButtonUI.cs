using System;
using UnityEngine;
using UnityEngine.Events;

public class ToggleButtonUI : ToggleButtonUIBase
{

    [SerializeField] private GameObject activeStateUI;
    [SerializeField] private GameObject deActiveStateUI;
    public Action ActiveEvent;
    protected override void Awake()
    {

        base.Awake();
        if (!activeStateUI)
        {
            Debug.LogWarning("activeStateUI is null");
        }

        if (!deActiveStateUI)
        {
            Debug.LogWarning("deActiveStateUI is null");
        }

        ChangeState(currentState);
    }
    
    public void OnActive()
    {
        ActiveEvent?.Invoke();
    }

    public override void ChangeState(State newState)
    {
        base.ChangeState(newState);
        if (currentState == State.Active)
        {
            activeStateUI?.gameObject.SetActive(true);
            deActiveStateUI?.gameObject.SetActive(false);
        }
        else
        {
            activeStateUI?.gameObject.SetActive(false);
            deActiveStateUI?.gameObject.SetActive(true);
        }

    }
}