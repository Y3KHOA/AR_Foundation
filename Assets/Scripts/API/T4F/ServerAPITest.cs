using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class ServerAPITest : MonoBehaviour
{
    [Header("UI Elements")]
    public Button CheckServerButton;        // Button OK
    public Button PostButton;               // Button Post
    public TMP_InputField InputField;       // Input text
    public TextMeshProUGUI ResultText;      // Text nhận kết quả

    // Thay link này bằng link ngrok mới nhất
    [Header("Server Config")]
    [SerializeField] private string baseURL = "https://1b75-113-172-83-10.ngrok-free.app";

    void Start()
    {
        CheckServerButton.onClick.AddListener(CheckServer);
        PostButton.onClick.AddListener(PostInputText);
    }

    void CheckServer()
    {
        Debug.Log("CheckServer clicked");
        StartCoroutine(CallPing());
    }

    void PostInputText()
    {
        string inputText = InputField.text.Trim();
        if (!string.IsNullOrEmpty(inputText))
        {
            Debug.Log($"PostInputText: {inputText}");
            StartCoroutine(CallPost(inputText));
        }
        else
        {
            Debug.LogWarning("Input is empty");
            ResultText.text = "Please enter some text!";
        }
    }

    IEnumerator CallPing()
    {
        string url = $"{baseURL}/ping";
        Debug.Log($"Ping URL: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Bypass ngrok warning
            request.SetRequestHeader("ngrok-skip-browser-warning", "any");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"Ping Error: {request.error}");
                ResultText.text = "Ping failed!";
            }
            else
            {
                Debug.Log($"Ping Response: {request.downloadHandler.text}");
                ResultText.text = $"Ping OK:\n{request.downloadHandler.text}";
            }
        }
    }

    IEnumerator CallPost(string textToSend)
    {
        string url = $"{baseURL}/predict";
        Debug.Log($"Post URL: {url}");

        string jsonBody = $"{{\"text\":\"{textToSend}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            // Bypass ngrok warning
            request.SetRequestHeader("ngrok-skip-browser-warning", "any");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"Post Error: {request.error}");
                ResultText.text = "Post failed!";
            }
            else
            {
                Debug.Log($"Post Response: {request.downloadHandler.text}");
                ResultText.text = $"Post OK:\n{request.downloadHandler.text}";
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("Stopping all coroutines to prevent GC handle leaks");
        StopAllCoroutines();
    }
}
