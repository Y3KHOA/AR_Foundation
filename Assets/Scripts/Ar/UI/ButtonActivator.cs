using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ButtonActivator : MonoBehaviour
{
    public Button targetButton; // Button cần bật
    public BtnController btnController; // Script chứa dữ liệu tọa độ

    void Start()
    {
        if (targetButton != null)
            targetButton.onClick.AddListener(OnTargetButtonClicked);
    }

    void Update()
    {
        if (btnController == null)
        {
            Debug.LogError("Chưa gán BtnController trong ButtonActivator!");
            return;
        }

        if (btnController.Flag == 1 && !targetButton.gameObject.activeSelf)
        {
            targetButton.gameObject.SetActive(true);
            Debug.Log("Button đã được bật!");
        }
    }

    void OnTargetButtonClicked()
    {
        Debug.Log("Button được nhấn - Chuyển dữ liệu và sang DraftingScene");
        TransferData();
        SceneManager.LoadScene("DraftingScene");
    }

    void TransferData()
    {
        List<Vector3> basePoints = btnController.GetBasePoints();
        List<Vector3> heightPoints = btnController.GetHeightPoints();
        List<Vector2> projectedPoints = new List<Vector2>();
        List<float> heightValues = new List<float>();

        for (int i = 0; i < basePoints.Count; i++)
        {
            projectedPoints.Add(new Vector2(basePoints[i].x, basePoints[i].z));
            heightValues.Add(heightPoints[i].y - basePoints[i].y); // Tính chiều cao thực tế
        }

        DataTransfer.Instance.SetPoints(projectedPoints);
        DataTransfer.Instance.SetHeights(heightValues);

        Debug.Log($"[TransferData] da luu {projectedPoints.Count} diem va {heightValues.Count} chieu cao!");
    }
}

public class DataTransfer
{
    private static DataTransfer instance;
    private List<Vector2> points;
    private List<float> heights;

    private DataTransfer()
    {
        points = new List<Vector2>();
        heights = new List<float>();
    }

    public static DataTransfer Instance
    {
        get
        {
            if (instance == null)
                instance = new DataTransfer();
            return instance;
        }
    }

    public void SetPoints(List<Vector2> newPoints)
    {
        points = newPoints;
    }

    public List<Vector2> GetPoints()
    {
        return points;
    }

    public void SetHeights(List<float> newHeights)
    {
        heights = newHeights;
    }

    public List<float> GetHeights()
    {
        return heights;
    }
}
