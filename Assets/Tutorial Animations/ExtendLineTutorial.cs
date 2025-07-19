using UnityEngine;

public class ExtendLineTutorial : BaseLineTutorial
{
    public Transform startPoint;
    public Transform endPoint;

    private void Update()
    {
        if (!startPoint || !endPoint) return;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        Vector3 pointOnLine = Vector3.Lerp(start, end, ratio);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, pointOnLine);
    }
}