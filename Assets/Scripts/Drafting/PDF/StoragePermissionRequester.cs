using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class StoragePermissionRequester : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject popupPanel;           // Gán Panel chứa popup UI
    public Button allowButton;              // Gán button "Cho phép"
    public Button denyButton;               // Gán button "Từ chối"

    private bool waitingForPermission = false;

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false); // Tắt popup khi bắt đầu

        if (allowButton != null)
            allowButton.onClick.AddListener(OnAllowClicked);
        if (denyButton != null)
            denyButton.onClick.AddListener(OnDenyClicked);
    }

    public void RequestAllFilesAccessWithPopup()
    {
        if (IsAllFilesAccessGranted())
        {
            Debug.Log("Đã có quyền truy cập.");
            return;
        }

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    private void OnAllowClicked()
    {
        popupPanel.SetActive(false);
        waitingForPermission = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier);

                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION");
                intent.Call<AndroidJavaObject>("setData", uri);

                activity.Call("startActivity", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("All Files Access Err: " + e.Message);
        }
#endif
    }

    private void OnDenyClicked()
    {
        popupPanel.SetActive(false);
        Debug.Log("access success");
    }

    public bool IsAllFilesAccessGranted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass env = new AndroidJavaClass("android.os.Environment");
            return env.CallStatic<bool>("isExternalStorageManager");
        }
        catch (System.Exception e)
        {
            Debug.LogError("check permission err: " + e.Message);
            return false;
        }
#else
        return true;
#endif
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && waitingForPermission)
        {
            waitingForPermission = false;

            if (IsAllFilesAccessGranted())
            {
                Debug.Log("Đã được cấp quyền → có thể lưu file!");
                // Gọi callback hoặc thực hiện hành động tiếp theo ở đây nếu muốn
            }
            else
            {
                Debug.LogWarning("Người dùng chưa cấp quyền truy cập toàn bộ bộ nhớ.");
            }
        }
    }
}
