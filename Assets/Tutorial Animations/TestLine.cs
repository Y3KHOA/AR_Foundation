using DG.Tweening;
using UnityEngine;

public class TestLine : BaseLineTutorial
{
    private Vector3 targetValue;
    private Vector3 startValue;

    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        startValue = lineRenderer.GetPosition(0);
        targetValue = lineRenderer.GetPosition(1);
    }

    private Tween tween;

    public void Run(float time = 1)
    {
        Debug.Log("Run " + gameObject.name, gameObject);
        tween?.Kill();
        tween = DOVirtual.Vector3(startValue, targetValue, time, value => { lineRenderer.SetPosition(1, value); });
    }



    public void ResetLine()
    {
        tween?.Kill();
        lineRenderer.SetPosition(1, startValue);
    }
}