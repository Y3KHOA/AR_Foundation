using System.Collections.Generic;
using UnityEngine;
using TMPro;
/*
Chịu trách nhiệm vẽ line giữa các checkpoint.

Cập nhật khoảng cách giữa các checkpoint khi có thay đổi.

Tách ra từ:
- DrawLineAndDistance()
- UpdateLinesAndDistances()
*/

public class LineManager : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;
    public List<GameObject> lines = new List<GameObject>();
    public List<GameObject> distanceTexts = new List<GameObject>();

    public void DrawLineAndDistance(Vector3 start, Vector3 end, GameObject distanceTextPrefab)
    {
        if (linePrefab == null || distanceTextPrefab == null)
        {
            Debug.LogError("Khong Prefab cho line Or distanceText!");
            return;
        }

        GameObject lineObj = Instantiate(linePrefab);
        LineRenderer line = lineObj.GetComponent<LineRenderer>();

        if (line != null)
        {
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            lines.Add(lineObj);
        }
        else
        {
            Debug.LogError("linePrefab khong co LineRenderer!");
        }

        // Tính khoảng cách
        float distanceInMeters = Vector3.Distance(start, end);
        float distanceInCm = distanceInMeters * 100f;

        Debug.Log($"Cạnh {lines.Count}: {distanceInCm:F1} cm");

        // Hiển thị khoảng cách trên TextMeshPro
        Vector3 midPoint = (start + end) / 2;
        GameObject textObj = Instantiate(distanceTextPrefab, end, Quaternion.identity);
        TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

        if (textMesh != null)
        {
            textMesh.text = $"{distanceInCm:F1} cm";
            textMesh.alignment = TextAlignmentOptions.Center;

            // Đặt text luôn nhìn về camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                textObj.transform.LookAt(mainCamera.transform);
                textObj.transform.Rotate(0, 180, 0); // Quay ngược lại để không bị ngược chữ
            }
        }
        else
        {
            Debug.LogError("distanceTextPrefab khong co TextMeshPro!");
        }
        distanceTexts.Add(textObj);
    }

    public void UpdateLinesAndDistances(List<GameObject> checkpoints)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            LineRenderer line = lines[i].GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, checkpoints[i].transform.position);
                line.SetPosition(1, checkpoints[(i + 1) % checkpoints.Count].transform.position);
            }

            // Cập nhật khoảng cách hiển thị
            if (i < distanceTexts.Count)
            {
                float distanceInMeters = Vector3.Distance(checkpoints[i].transform.position, checkpoints[(i + 1) % checkpoints.Count].transform.position);
                float distanceInCm = distanceInMeters * 100f;

                TextMeshPro tmp = distanceTexts[i].GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = $"{distanceInCm:F1} cm";
                    Debug.Log($"Canh {i + 1} update: {distanceInCm:F1} cm");
                }
            }
        }
    }
}
