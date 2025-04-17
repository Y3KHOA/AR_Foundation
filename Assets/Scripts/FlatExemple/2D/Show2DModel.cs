using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Show2DModel : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject checkpointPrefab;
    public Drawing2D Drawing2D;
    // public Transform panelFloor;

    private List<List<GameObject>> allCheckpoints = new List<List<GameObject>>();
    private float closeThreshold = 0.5f; // Khoảng cách tối đa để chọn điểm
    private bool isClosedLoop = false; // Biến kiểm tra xem mạch đã khép kín chưa

    void Start()
    {
        LoadPointsFromDataTransfer();
    }

    void Update()
    {
    }

    void LoadPointsFromDataTransfer()
    {
        List<List<Vector2>> allPoints = DataTransfer.Instance.GetAllPoints();
        List<List<float>> allHeights = DataTransfer.Instance.GetAllHeights();

        if (allPoints.Count == 0)
        {
            Debug.Log("Không có dữ liệu điểm để hiển thị.");
            return;
        }

        for (int pathIndex = 0; pathIndex < allPoints.Count; pathIndex++)
        {
            List<Vector2> path = allPoints[pathIndex];
            List<GameObject> checkpointsForPath = new List<GameObject>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = new Vector3(path[i].x, 0, path[i].y); // Y = 0 vì hiển thị 2D
                GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity);
                // GameObject checkpoint = Instantiate(checkpointPrefab, worldPos, Quaternion.identity, panelFloor.transform);

                checkpointsForPath.Add(checkpoint);

                // Nếu có ít nhất 2 điểm, vẽ line giữa các điểm
                if (i > 0)
                {
                    Drawing2D.DrawLineAndDistance(checkpointsForPath[i - 1].transform.position, worldPos);
                }
            }

            // Tự động nối kín nếu đủ điểm và 2 đầu gần nhau
            if (checkpointsForPath.Count > 2 && Vector3.Distance(checkpointsForPath[0].transform.position, checkpointsForPath[^1].transform.position) < closeThreshold)
            {
                Drawing2D.DrawLineAndDistance(checkpointsForPath[^1].transform.position, checkpointsForPath[0].transform.position);
                isClosedLoop = true;
            }

            // Lưu vào list tổng
            allCheckpoints.Add(checkpointsForPath);
        }

        Debug.Log($"[LoadPoints] Đã nạp {allPoints.Count} mạch với tổng cộng {CountTotalCheckpoints()} checkpoint.");
    }
    int CountTotalCheckpoints()
    {
        int total = 0;
        foreach (var list in allCheckpoints)
            total += list.Count;
        return total;
    }
}
