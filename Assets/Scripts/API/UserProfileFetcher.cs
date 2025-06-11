using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class UserProfileFetcher : MonoBehaviour
{
    [Header("Gắn các TextMeshProUGUI để hiển thị dữ liệu")]
    public TextMeshProUGUI tokenText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI phoneText;
    public TextMeshProUGUI addressText;
    public TextMeshProUGUI birthText;

    [ContextMenu("Fetch User Profile")]
    public void FetchUserProfile()
    {
        StartCoroutine(GetUserProfile());
    }

    IEnumerator GetUserProfile()
    {
        string token = SessionData.token;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Token rỗng, không thể gọi API.");
            yield break;
        }

        string url = "https://apis-dev.xheroapp.com/users/me"; // Đặt đúng endpoint để lấy profile

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json"); 

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Du lieu user: " + request.downloadHandler.text);
            RootResponse response = JsonUtility.FromJson<RootResponse>(request.downloadHandler.text);

            // Gán thông tin vào UI
            tokenText.text = "Token: " + token;
            nameText.text = "Họ tên: " + response.data.user.fullName;
            phoneText.text = "SDT: " + response.data.user.username;
            addressText.text = "Địa chỉ: " + (response.data.user.address.Length > 0 ? response.data.user.address[0] : "N/A");
            birthText.text = "Ngày sinh: " + (response.data.user.birthDay.Length > 0 ? response.data.user.birthDay[0] : "N/A");
        }
        else
        {
            Debug.LogError("Loi goi API profile: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    // Dùng lại cấu trúc lớp từ script bạn đã cung cấp:
    [System.Serializable]
    public class UserData
    {
        public string username;
        public string fullName;
        public string[] birthDay;
        public string[] address;
    }

    [System.Serializable]
    public class Data
    {
        public string token;
        public UserData user;
    }

    [System.Serializable]
    public class RootResponse
    {
        public bool status;
        public Data data;
    }
}
