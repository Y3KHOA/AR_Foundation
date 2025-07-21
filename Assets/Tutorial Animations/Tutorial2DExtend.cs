using System.Collections.Generic;
using UnityEngine;

public class Tutorial2DExtend : TutorialBase
{
    [SerializeField] private ExtendLineTutorial linePrefab;
    [SerializeField] private Transform[] points;
    [SerializeField] private List<ExtendLineTutorial> lineRenderers = new();
    [SerializeField] private Transform mouseContainer;
    private Vector3 originalPosition;
    
    public override void SetRatio(float ratio)
    {
        base.SetRatio(ratio);
        foreach (var item in lineRenderers)
        {
            item.SetRatio(ratio);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < points.Length; i++)
        {
            var line = Instantiate(linePrefab);
            line.GetComponent<LineRenderer>().useWorldSpace = true;
            line.gameObject.SetActive(true);
            line.startPoint = points[i];
            line.endPoint = points[i + 1 >= points.Length ? 0 : i + 1];
            lineRenderers.Add(line);
        }

        originalPosition = points[0].transform.localPosition;
    }

    private void Update()
    {
        foreach (var item in lineRenderers)
        {
            item.UpdateLine();
        }
    }

    public void AddPointParent()
    {
        points[0].transform.SetParent(mouseContainer.transform);
    }

    public void RemovePointParent()
    {
        points[0].transform.parent = points[1].transform.parent;
        points[0].transform.localPosition = originalPosition;
    }
    
    public override void StopTutorial()
    {
        GetComponent<Animator>().Play("Idle");
        RemovePointParent();
        mouseContainer.gameObject.SetActive(false);

        foreach (var item in lineRenderers)
        {
            item.gameObject.SetActive(false);
        }
        foreach (var item in points)
        {
            item.gameObject.SetActive(false);
        }

    }

    public override void PlayTutorial()
    {
        
        GetComponent<Animator>().Play("Tutorial");
        mouseContainer.gameObject.SetActive(true);
        foreach (var item in lineRenderers)
        {
            item.gameObject.SetActive(true);
        }
        
        foreach (var item in points)
        {
            item.gameObject.SetActive(true);
        }
    }
}