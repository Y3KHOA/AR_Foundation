using UnityEngine;
using UnityEngine.Networking;
using System;

public static class SessionData
{
    public static string token;
}

public class CrossAppLoginHandler : MonoBehaviour
{
    [Header("Cấu hình URI của 2 app")]
    public string xheroScheme = "xhero://xhero.deeplink"; // URI gọi App XHero
    public string callbackScheme = "xheroscan://auth/callback"; // URI scheme của app này

    void Start()
    {
        // Lắng nghe deeplink trả về (khi app đang chạy)
        Application.deepLinkActivated += OnDeepLinkActivated;

        // Trường hợp app được mở bằng deeplink từ trạng thái "đã tắt"
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    public void OnLoginButtonClicked()
    {
        // Gọi app XHero bằng URI Scheme và truyền callback URI để nhận token
        string fullUri = $"{xheroScheme}?callback={UnityWebRequest.EscapeURL(callbackScheme)}";
        Debug.Log("Gui yeu cau den XHero: " + fullUri);
        Application.OpenURL(fullUri);
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("Duoc mo URL: " + url);
        Uri uri = new Uri(url);

        // Parse query string: xheroscan://auth/callback?token=abc.def.ghi
        string query = uri.Query; // ?token=...
        string token = ExtractToken(query);
        if (!string.IsNullOrEmpty(token))
        {
            SessionData.token = token;
            Debug.Log("Da luu token: " + token);
        }
        else
        {
            Debug.LogWarning("Ko tim thay token trong deeplink.");
        }
    }

    private string ExtractToken(string query)
    {
        if (query.StartsWith("?")) query = query.Substring(1); // bỏ dấu '?'
        string[] parts = query.Split('&');
        foreach (string part in parts)
        {
            string[] kv = part.Split('=');
            if (kv.Length == 2 && kv[0] == "token")
            {
                return Uri.UnescapeDataString(kv[1]);
            }
        }
        return null;
    }
}
