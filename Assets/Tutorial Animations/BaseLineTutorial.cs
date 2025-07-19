using UnityEngine;

public class BaseLineTutorial : MonoBehaviour
{
    [SerializeField] protected LineRenderer lineRenderer;
    [SerializeField] protected float ratio = 1;
    protected float startWidth = 0.1f;

    protected virtual void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    
    public void SetRatio(float ratio)
    {
        this.ratio = ratio;
        lineRenderer.startWidth = Mathf.Clamp(startWidth * ratio, startWidth, 100);
        lineRenderer.endWidth = Mathf.Clamp(startWidth * ratio, startWidth, 100);
    }
}