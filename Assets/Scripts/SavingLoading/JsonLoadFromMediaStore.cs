using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;
using System.Text;

public class JsonLoadFromMediaStore : MonoBehaviour
{
    private const int PICK_JSON_FILE_REQUEST_CODE = 42;
    private AndroidJavaObject currentActivity;
    private Action<string> onJsonLoaded;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
    }

    public void TriggerPickJsonFile(Action<string> callback)
    {
        onJsonLoaded = callback;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.OPEN_DOCUMENT");
        intent.Call<AndroidJavaObject>("setType", "application/json");
        intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");

        currentActivity.Call("startActivityForResult", intent, PICK_JSON_FILE_REQUEST_CODE);
#endif
    }

    // Cần cài thêm Plugin xử lý OnActivityResult nếu muốn nhận Uri phản hồi từ hệ thống Android
}
