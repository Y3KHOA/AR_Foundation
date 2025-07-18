using DG.Tweening;
using UnityEngine;

public class TestLine : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float ratio = 1;
    private Vector3 targetValue;
    private Vector3 startValue;

    private float startWidth = 0.1f;
    
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

    public void SetRatio(float ratio)
    {
        this.ratio = ratio;
        lineRenderer.startWidth = Mathf.Clamp(startWidth * ratio, startWidth, 100);
        lineRenderer.endWidth = Mathf.Clamp(startWidth * ratio, startWidth, 100);
    }

    public void ResetLine()
    {
        tween?.Kill();
        lineRenderer.SetPosition(1, startValue);
    }
}