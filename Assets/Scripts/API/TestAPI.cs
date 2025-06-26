using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class LoginResponse
{
    public string token;
    public string name;
    public string phone;
    public string address;
    public string birth;
}

public class TestAPI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI tokenText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI phoneText;
    public TextMeshProUGUI addressText;
    public TextMeshProUGUI birthText;

    public void OnLoginButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        StartCoroutine(LoginCoroutine(username, password));
    }

    IEnumerator LoginCoroutine(string username, string password)
    {
        string url = "https://apis-dev.xheroapp.com/users/authenticate";

        // Tạo object request đúng format
        LoginRequest requestBody = new LoginRequest
        {
            username = username,
            password = password,
            device = "UnityClient",
            deviceToken = "mockToken123",
            fromPlatform = null // giữ null nếu backend chấp nhận null
        };

        string json = JsonUtility.ToJson(requestBody);
        Debug.Log("JSON gửi đi: " + json);  // Debug in ra nếu cần kiểm tra

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Đăng nhập thành công: " + request.downloadHandler.text);

            RootResponse response = JsonUtility.FromJson<RootResponse>(request.downloadHandler.text);

            // Gán thông tin từ response vào UI
            tokenText.text = "Token: " + response.data.token;
            nameText.text = "Họ tên: " + response.data.user.fullName;
            phoneText.text = "SDT: " + response.data.user.username;
            addressText.text = "Địa chỉ: " + (response.data.user.address.Length > 0 ? response.data.user.address[0] : "N/A");
            birthText.text = "Ngày sinh: " + (response.data.user.birthDay.Length > 0 ? response.data.user.birthDay[0] : "N/A");

        }
        else
        {
            Debug.LogError("Lỗi khi gọi API: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
        public string device;
        public string deviceToken;
        public string fromPlatform;
    }

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
