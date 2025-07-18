﻿using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TutorialBase : MonoBehaviour
{
    protected bool isPlayAgain = false;

    protected virtual void Awake()
    {
        TutorialHandle.tutorial = this;
    }

    public virtual void SetRatio(float ratio)
    {
        
    }
    public abstract void StopTutorial();

    public abstract void PlayTutorial();


}