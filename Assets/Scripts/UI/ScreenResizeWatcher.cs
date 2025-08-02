using System;
using UnityEngine;

public class ScreenResizeWatcher : MonoBehaviour
{
    public static ScreenResizeWatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ScreenResizeWatcher");
                _instance = go.AddComponent<ScreenResizeWatcher>();
                DontDestroyOnLoad(go); 
            }
            return _instance;
        }
    }

    private static ScreenResizeWatcher _instance;

    private int lastWidth;
    private int lastHeight;

   
    public event Action<int, int> OnScreenResize;
    public event Action OnScreenResizeEvent;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Ngăn nhiều instance
            return;
        }

        _instance = this;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            OnScreenResize?.Invoke(Screen.width, Screen.height);
            OnScreenResizeEvent?.Invoke();
        }
    }
}