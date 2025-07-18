﻿using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialHandle : MonoBehaviour
{
    public static TutorialBase tutorial;
    private Camera mainCam;
    public GameObject inputPanel;
    [SerializeField] private float originalSize = 5;
    [SerializeField] private float ratio = 5;

    public float Ratio => ratio;
    
    public void Play()
    {
        tutorial.transform.position = GetCenterPos(tutorial.transform);
        ProcessRatio();
        tutorial.SetRatio(ratio);
        tutorial?.PlayTutorial();
        inputPanel.gameObject.SetActive(true);
    }

    public void Stop()
    {
        tutorial?.StopTutorial();
        inputPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        mainCam = Camera.main;
        Play();
        originalSize = mainCam.orthographicSize;
    }

    private void ProcessRatio()
    {
        ratio = mainCam.orthographicSize / originalSize;
        tutorial.transform.localScale = Vector3.one * ratio;
    }


    
    
    private void Update()
    {
        SetToCenterOfCamera();
    }

    private void SetToCenterOfCamera()
    {
        var center = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
    }
    
    
    private Vector3 GetCenterPos(Transform target)
    {
        var center = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
        return new Vector3(center.x, target.transform.position.y, center.z);
    }
}