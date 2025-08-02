using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundUI : MonoBehaviour
{
    private static BackgroundUI instance;

    private Image background;
    private Canvas canvas;

    private Action clickCallback;

    public static BackgroundUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<BackgroundUI>();
                instance.gameObject.name = "Background UI";
            }

            return instance;
        }
        set { instance = value; }
    }


    public BackgroundUI()
    {
    }

    private void SceneManagerOnsceneUnloaded(Scene arg0)
    {
        if (background)
            background.transform.parent = null;
    }

    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        // TryInitBackground();
        // canvas = GameObject.FindFirstObjectByType<Canvas>();
        // background.gameObject.transform.parent = canvas.transform;
        // ResetEverything();
    }

    private Coroutine playDelayCoroutine;
    
    public void Show(GameObject target, Action onClickCallback)
    {
        if (playDelayCoroutine != null)
        {
            CoroutineManager.Stop(playDelayCoroutine);
        }
        clickCallback = onClickCallback;
        playDelayCoroutine = CoroutineManager.Run(PlayDelay(target));
    }
    

    private IEnumerator PlayDelay(GameObject target)
    {
        TryInitBackground();

        yield return new WaitForEndOfFrame();

        if (target)
        {
            background.transform.SetParent(target.transform.parent.transform);
            int targetIndex = target.transform.GetSiblingIndex();
            int newIndex = Mathf.Max(0, targetIndex - (background.transform.GetSiblingIndex() < targetIndex ? 1 : 0));
            background.transform.SetSiblingIndex(newIndex);
        }
        else
        {
            Debug.Log("Target is null, please checkout");
        }

        SetBackgroundActive(true);
        yield return null;
    }
    
    private void TryInitBackground()
    {
        if (!background)
        {
            background = new GameObject().AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f);
            background.raycastTarget = true;
            background.gameObject.name = "Background Black";

            canvas = GameObject.FindFirstObjectByType<Canvas>();

            background.transform.SetParent(canvas.transform);

            ResetEverything();

            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.transform.rotation = canvas.transform.rotation;

            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;

            backgroundRect.offsetMax = new Vector2(500, 500);
            backgroundRect.offsetMin = new Vector2(-500, -500);

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => OnClick((PointerEventData)eventData));

            var eventTrigger = backgroundRect.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(entry);

            SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
            backgroundRect.gameObject.SetActive(false);
        }
    }

    private void OnClick(PointerEventData data)
    {
        Debug.Log("On pointer click");
        if (clickCallback != null)
        {
            clickCallback?.Invoke();
            clickCallback = null;
        }
    }

    public void Hide()
    {
        if (playDelayCoroutine != null)
        {
            CoroutineManager.Stop(playDelayCoroutine);
        }
        SetBackgroundActive(false);
    }

    private void SetBackgroundActive(bool isActive)
    {
        if (background == null) return;
        background.gameObject.SetActive(isActive);
    }


    private void ResetEverything()
    {
        background.transform.localScale = Vector3.one;
        background.transform.localPosition = Vector3.zero;
    }
}