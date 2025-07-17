using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonUIBase : MonoBehaviour
{
    public Button btn;
    public State currentState = State.DeActive;
    public Action<State> OnValueChange;

    protected virtual void Awake()
    {
        btn = GetComponent<Button>();
    }
    
    public enum State
    {
        Active,
        DeActive
    }

    public virtual void ChangeState(State newState)
    {
        currentState = newState;
        OnValueChange?.Invoke(currentState);
    }

    public void Toggle()
    {
        if (currentState == State.Active)
        {
            currentState = State.DeActive;
        }
        else
        {
            currentState = State.Active;
        }

        ChangeState(currentState);
    }
}