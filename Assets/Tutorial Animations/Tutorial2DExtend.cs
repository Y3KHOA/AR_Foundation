using System.Collections.Generic;
using UnityEngine;

public class Tutorial2DExtend : TutorialBase
{
    [SerializeField] private ExtendLineTutorial linePrefab;
    [SerializeField] private Transform[] points;
    [SerializeField] private List<ExtendLineTutorial> lineRenderers = new();
    [SerializeField] private Transform mouseContainer;
    public override void SetRatio(float ratio)
    {
        base.SetRatio(ratio);
        foreach (var item in lineRenderers)
        {
            item.SetRatio(ratio);

            
        }
    }

    public void AddParent()
    {
        points[0].transform.SetParent(mouseContainer.transform);
    }

    public void RemoveParent()
    {
        points[0].transform.parent = null;
    }
    
    public override void StopTutorial()
    {
    }

    public override void PlayTutorial()
    {
        for (int i = 0; i < points.Length; i++)
        {
            var line = Instantiate(linePrefab);
            line.GetComponent<LineRenderer>().useWorldSpace = true;
            line.gameObject.SetActive(true);
            line.startPoint = points[i];
            line.endPoint = points[i + 1 >= points.Length ? 0 : i + 1];
            lineRenderers.Add(line);
        }
    }
}