using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;

public class CompassManager : MonoBehaviour
{
    [Header("Object References")]
    public Camera ReferenceCamera;
    public TextMeshProUGUI compassObject; // Mũi tên hoặc chữ "N" trong AR

    private string direction = "";
    private float currentHeading = 0f;
    private Queue<float> headingHistory = new Queue<float>();
    private const int smoothWindowSize = 5;
    public static CompassManager Instance;
    void Awake()
    {
        Instance = this;
    }
    public float GetCurrentHeading()
    {
        return currentHeading;
    }

    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            StartCoroutine(WaitForPermission());
        }
        else
        {
            StartCoroutine(InitializeAfterDelay());
        }
    }

    private IEnumerator WaitForPermission()
    {
        // Đợi cho user bấm Allow hoặc Deny
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null; // đợi 1 frame
        }

        // Nếu user bấm Allow thì reload
        Debug.Log("[Compass] Permission granted — reload scene now.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private IEnumerator InitializeAfterDelay()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            // Đợi user phản hồi (1 frame)
            yield return null;

            // Nếu vẫn chưa được cấp, thoát
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.LogError("[Compass] Permission denied for FineLocation.");
                yield break;
            }
        }

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("[Compass] Location services are not enabled by the user.");
            yield break;
        }

        Debug.Log("[Compass] Stopping location (if running)...");
        Input.location.Stop();
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[Compass] Starting location...");
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError("[Compass] Location service failed to start.");
            yield break;
        }

        // Bây giờ mới bật compass
        Input.compass.enabled = true;

        Debug.Log("[Compass] Compass enabled.");

        // Đợi dữ liệu compass cập nhật
        int compassWait = 10;
        while (Input.compass.timestamp == 0 && compassWait > 0)
        {
            Debug.Log("[Compass] Waiting for compass to be ready...");
            yield return new WaitForSeconds(1);
            compassWait--;
        }

        if (Input.compass.timestamp == 0)
        {
            Debug.LogError("[Compass] Compass failed to initialize (timestamp = 0).");
            yield break;
        }

        Debug.Log("[Compass] Compass and location initialized successfully.");
    }

    void Update()
    {
        if (!Input.compass.enabled || Input.compass.timestamp == 0)
            return;

        float rawHeading = Input.compass.trueHeading;
        rawHeading = Mathf.Repeat(rawHeading, 360f);

        // Lưu vào hàng đợi
        if (headingHistory.Count >= smoothWindowSize)
            headingHistory.Dequeue();
        headingHistory.Enqueue(rawHeading);

        // Tính trung bình góc (mục tiêu mới)
        float smoothedTarget = AverageAngles(headingHistory);

        // Làm mượt giữa currentHeading → smoothedTarget
        currentHeading = Mathf.LerpAngle(currentHeading, smoothedTarget, Time.deltaTime * 3f); // Bạn có thể thử giá trị từ 3f - 10f
        currentHeading = Mathf.Repeat(currentHeading, 360f);


        string direction = GetDirectionFromDegree(currentHeading);

        if (compassObject != null)
        {
            compassObject.text = $"{direction}:{currentHeading:F1}\u00B0";
        }
    }
    private float AverageAngles(IEnumerable<float> angles)
    {
        float x = 0f, y = 0f;
        foreach (var angle in angles)
        {
            float rad = angle * Mathf.Deg2Rad;
            x += Mathf.Cos(rad);
            y += Mathf.Sin(rad);
        }

        float avgAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (avgAngle + 360f) % 360f;
    }

    private string GetDirectionFromDegree(float degree)
    {
        if (degree < 0) degree += 360;

        if ((degree >= 0 && degree < 7.5f) || degree >= 352.5f) return "Bắc";
        if (degree < 22.5f) return "Bắc";
        if (degree < 37.5f) return "Đông Bắc";
        if (degree < 52.5f) return "Đông Bắc";
        if (degree < 67.5f) return "Đông Bắc";
        if (degree < 82.5f) return "Đông";
        if (degree < 97.5f) return "Đông";
        if (degree < 112.5f) return "Đông";
        if (degree < 127.5f) return "Đông Nam";
        if (degree < 142.5f) return "Đông Nam";
        if (degree < 157.5f) return "Đông Nam";
        if (degree < 172.5f) return "Nam";
        if (degree < 187.5f) return "Nam";
        if (degree < 202.5f) return "Nam";
        if (degree < 217.5f) return "Tây Nam";
        if (degree < 232.5f) return "Tây Nam";
        if (degree < 247.5f) return "Tây Nam";
        if (degree < 262.5f) return "Tây";
        if (degree < 277.5f) return "Tây";
        if (degree < 292.5f) return "Tây";
        if (degree < 307.5f) return "Tây Bắc";
        if (degree < 322.5f) return "Tây Bắc";
        if (degree < 337.5f) return "Tây Bắc";
        return "Bắc";
    }

}
