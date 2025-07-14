using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundUI : MonoBehaviour
{
    private static BackgroundUI instance;

    private Image background;
    private Canvas canvas;

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
        background = new GameObject().AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.5f);
        background.raycastTarget = false;

        canvas = GameObject.FindFirstObjectByType<Canvas>();
        background.transform.SetParent(canvas.transform);
        ResetEverything();

        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;
        backgroundRect.transform.rotation = canvas.transform.rotation;  

        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
    }

    private void SceneManagerOnsceneUnloaded(Scene arg0)
    {
        background.transform.parent = null;
    }

    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        canvas = GameObject.FindFirstObjectByType<Canvas>();
        background.gameObject.transform.parent = canvas.transform;
        ResetEverything();
    }

    public void Show(GameObject target)
    {
        if (target)
        {
            background.transform.SetParent(canvas.transform);
            background.transform.SetSiblingIndex(target.transform.GetSiblingIndex() - 1);
        }
        SetBackgroundActive(true);
    }

    public void Hide()
    {
        SetBackgroundActive(false);
    }

    private void SetBackgroundActive(bool isActive)
    {
        background.gameObject.SetActive(isActive);
    }


    private void ResetEverything()
    {
        background.transform.localScale = Vector3.one;
        background.transform.localPosition = Vector3.zero;
    }
}