using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;

    private static CoroutineManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[CoroutineManager]");
                _instance = go.AddComponent<CoroutineManager>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }

    public static void Stop(IEnumerator coroutine)
    {
        if (_instance != null)
        {
            _instance.StopCoroutine(coroutine);
        }
    }

    public static void Stop(Coroutine coroutine)
    {
        if (_instance != null)
        {
            _instance.StopCoroutine(coroutine);
        }
    }
}