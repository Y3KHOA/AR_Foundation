using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButtonUI : MonoBehaviour
{
    public Button btn;
    public State currentState = State.DeActive;
    [SerializeField] private GameObject activeStateUI;
    [SerializeField] private GameObject deActiveStateUI;

    public Action ActiveEvent;
    private void Awake()
    {
        btn = GetComponent<Button>();

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

    public void ChangeState(State newState)
    {
        currentState = newState;
        if (currentState == State.Active)
        {
            activeStateUI.gameObject.SetActive(true);
            deActiveStateUI.gameObject.SetActive(false);
        }
        else
        {
            activeStateUI.gameObject.SetActive(false);
            deActiveStateUI.gameObject.SetActive(true);
        }

    }
    
    public enum State
    {
        Active,
        DeActive
    }


    public void ChangeStateForVisual(State state)
    {
        currentState = state;
    }
}