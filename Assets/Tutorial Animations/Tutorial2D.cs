using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Splines;

public class Tutorial2D : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject mouse;
    [SerializeField] private TestLine[] lineRenderers;
    [SerializeField] private Transform[] points;
    [SerializeField] private Transform[] movePoints;
    [SerializeField] private EventTrigger eventTrigger;


    private bool isPlayAgain = false;
    
    [Serializable]
    public class CustomAnimation
    {
        public int activeIndex;
        public TestLine TestLine;
        public Transform points;
    }

    public List<CustomAnimation> customAnimations = new();
  
    private void Awake()
    {
        originalScale = points[0].transform.localScale;
        customAnimations = new List<CustomAnimation>()
        {
            new CustomAnimation(),
            new CustomAnimation(),
            new CustomAnimation(),
            new CustomAnimation(),
        };

        for (int i = 0; i < customAnimations.Count; i++)
        {
            customAnimations[i].activeIndex = i + 2;
            customAnimations[i].points = points[i];
            customAnimations[i].TestLine = lineRenderers[i];
        }
        
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(StopTutorial);
        eventTrigger.triggers.Add(entry);
    }

    private void StopTutorial(BaseEventData arg0)
    {
        sequence?.Kill();
        StopCoroutine(PlayTutorialSequence());
        eventTrigger.gameObject.SetActive(false);
        isPlayAgain = false;
        Clear();
    }

    private void Start()
    {
        PlayTutorial();
    }

    public void PlayTutorial()
    {
        if (isPlayAgain) return;
        eventTrigger.gameObject.SetActive(true);
        StartCoroutine(PlayTutorialSequence());
        isPlayAgain = true;
    }

    private Sequence sequence;
    private IEnumerator PlayTutorialSequence()
    {
        Clear();
        mouse.gameObject.SetActive(true);
    

        sequence = DOTween.Sequence();

        for (int index = 0; index < movePoints.Length; index++)
        {
            int currentIndex = index;
            var startPosition = movePoints[currentIndex].transform.position;

            sequence.Append(mouse.transform.DOMove(startPosition, 1).OnPlay(() =>
            {
                foreach (var item in customAnimations)
                {
                    if (item.activeIndex == currentIndex)
                    {
                        Debug.Log("Valid Index and run");
                        item.TestLine.Run(1);
                        CreateShowEffect(item.points);
                    }
                }
            }));
        }

        sequence.AppendCallback(() => { Clear(); });

        // Play and wait until done
        sequence.Play();
        yield return sequence.WaitForCompletion();

        // Gọi lại chính nó để lặp
        if(isPlayAgain)
            StartCoroutine(PlayTutorialSequence());
    }


    private Vector3 originalScale;
    private void Clear()
    {
        foreach (var item in lineRenderers) item.ResetLine();
        foreach (var point in points)
        {
            point.transform.GetComponent<SpriteRenderer>().DOFade(0, 0);
            point.DOScale(Vector3.zero,0);
        }
        mouse.gameObject.SetActive(false);
    }

    public void CreateShowEffect(Transform point)
    {
        point.transform.DOScale(originalScale, 0.3f).SetEase(Ease.InOutBack);
        point.transform.GetComponent<SpriteRenderer>().DOFade(1, 0.3f);
    }
}