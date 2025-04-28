using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; 
    public string nextSceneName = "SceneArchive"; 

    private bool videoStarted = false; // Biến kiểm tra xem video đã bắt đầu chưa
    private float videoStartTimeout = 2f; // Thời gian tối đa để video bắt đầu (3 giây)

    void Start()
    {
        try
        {
            if (videoPlayer != null)
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "myintro.mp4");

                // Kiểm tra và chỉnh sửa đường dẫn cho Android
                string videoUrl = "file://" + videoPath; // Dùng file:// để Android có thể truy cập

                Debug.Log("videoUrl mobile: " + videoUrl);

                // Kiểm tra môi trường, nếu trên Android thì sử dụng UnityWebRequest
                if (Application.platform == RuntimePlatform.Android)
                {
                    StartCoroutine(PlayVideoFromStreamingAssets(videoUrl)); // Chạy Coroutine ngoài try-catch
                }
                else
                {
                    if (System.IO.File.Exists(videoPath))
                    {
                        videoPlayer.url = videoUrl;
                        videoPlayer.loopPointReached += OnVideoEnd;
                        videoPlayer.Play();
                        videoStarted = true; // Đánh dấu video đã bắt đầu
                    }
                    else
                    {
                        Debug.LogError("Video file not found at path: " + videoPath);
                        SceneManager.LoadScene(nextSceneName);
                    }
                }

                // Bắt đầu đếm thời gian chờ
                StartCoroutine(VideoStartTimeout());
            }
            else
            {
                Debug.LogError("VideoPlayer component is not assigned.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
        catch (System.Exception ex)
        {
            // Catch any exceptions that occur inside the try block
            Debug.LogError("Exception caught: " + ex.Message);
            SceneManager.LoadScene(nextSceneName); // Nếu có lỗi, chuyển sang scene tiếp theo
        }
    }

    // Tải video từ StreamingAssets trên Android
    IEnumerator PlayVideoFromStreamingAssets(string videoUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(videoUrl); // Sử dụng videoUrl với file://
        yield return www.SendWebRequest();  // Đây là lệnh không đồng bộ, không thể trong try-catch

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Video loaded successfully from: " + videoUrl);
            videoPlayer.url = videoUrl;
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();
            videoStarted = true; // Đánh dấu video đã bắt đầu
        }
        else
        {
            Debug.LogError("Failed to load video: " + www.error);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // Đếm thời gian tối đa 3 giây, nếu video chưa chạy thì chuyển sang scene tiếp theo
    IEnumerator VideoStartTimeout()
    {
        float timer = 0f;

        // Trong vòng 3 giây nếu video chưa bắt đầu
        while (timer < videoStartTimeout && !videoStarted)
        {
            timer += Time.deltaTime;
            yield return null; // Tiến hành kiểm tra theo từng frame
        }

        if (!videoStarted) // Nếu video không bắt đầu sau 3 giây
        {
            Debug.LogWarning("Video didn't start within 3 seconds. Switching to next scene.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Load the next scene after the video ends
        SceneManager.LoadScene(nextSceneName);
    }
}
