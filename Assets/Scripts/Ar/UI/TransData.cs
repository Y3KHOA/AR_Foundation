using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TransData : MonoBehaviour
{

    public BtnController btnController; // Script chứa dữ liệu tọa độ

    public static TransData Instance { get; private set; }

    void Awake()
    {
        // Gán Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Nếu đã có một instance khác => hủy
        }
        else
        {
            Instance = this;
        }
    }

    public void TransferData()
    {
        // Lấy nested list từ BtnController
        List<List<GameObject>> allBasePoints = btnController.GetAllBasePoints();
        List<List<GameObject>> allHeightPoints = btnController.GetAllHeightPoints();

        List<List<Vector2>> allProjectedPoints = new List<List<Vector2>>();
        List<List<float>> allHeights = new List<List<float>>();

        for (int i = 0; i < allBasePoints.Count; i++)
        {
            List<Vector2> path2D = new List<Vector2>();
            List<float> heightList = new List<float>();

            for (int j = 0; j < allBasePoints[i].Count; j++)
            {
                Vector3 basePos = allBasePoints[i][j].transform.position;
                Vector3 heightPos = allHeightPoints[i][j].transform.position;

                path2D.Add(new Vector2(basePos.x, basePos.z));
                heightList.Add(heightPos.y - basePos.y);
            }

            allProjectedPoints.Add(path2D);
            allHeights.Add(heightList);
        }

        DataTransfer.Instance.SetAllPoints(allProjectedPoints);
        DataTransfer.Instance.SetAllHeights(allHeights);

        Debug.Log($"[TransferData] Đã lưu {allProjectedPoints.Count} mạch và tổng cộng {CountTotalPoints(allProjectedPoints)} điểm.");
    }

    int CountTotalPoints(List<List<Vector2>> data)
    {
        int total = 0;
        foreach (var list in data)
        {
            total += list.Count;
        }
        return total;
    }
}

public class DataTransfer
{
    private static DataTransfer instance;
    private List<List<Vector2>> allPoints;
    private List<List<float>> allHeights;

    private DataTransfer()
    {
        allPoints = new List<List<Vector2>>();
        allHeights = new List<List<float>>();
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

    public void SetAllPoints(List<List<Vector2>> newPoints)
    {
        allPoints = newPoints;
    }

    public List<List<Vector2>> GetAllPoints()
    {
        return allPoints;
    }

    public void SetAllHeights(List<List<float>> newHeights)
    {
        allHeights = newHeights;
    }

    public List<List<float>> GetAllHeights()
    {
        return allHeights;
    }
}
