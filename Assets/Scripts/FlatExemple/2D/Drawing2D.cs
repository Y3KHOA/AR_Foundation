using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Drawing2D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;
    public GameObject auxiliaryLinePrefab; // Line phụ
    public Transform modelRoot;

    private List<LineRenderer> linePool = new List<LineRenderer>(); // Object Pooling
    private List<TextMeshPro> textPool = new List<TextMeshPro>();
    private List<LineRenderer> auxiliaryLinesPool = new List<LineRenderer>(); // Pool line phụ

    public List<LineRenderer> lines = new List<LineRenderer>(); // Thêm danh sách này
    public List<TextMeshPro> distanceTexts = new List<TextMeshPro>();

    private float auxiliaryLineLength = 0.1f; // Độ dài line phụ (10cm)

    public void DrawLineAndDistance(Vector3 start, Vector3 end, Transform modelRoot)
    {
        if (linePrefab == null || distanceTextPrefab == null || auxiliaryLinePrefab == null)
        {
            Debug.LogError("Thiếu prefab line, text hoặc line phụ!");
            return;
        }

        // Vẽ line chính
        LineRenderer line = GetOrCreateLine();
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        lines.Add(line);

        float distanceInCm = Vector3.Distance(start, end) * 100f;

        // Tạo line phụ
        Vector3 dir = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.up).normalized; // Vuông góc với line chính

        Vector3 aux1Start = start - perpendicular * auxiliaryLineLength / 2;
        Vector3 aux1End = start + perpendicular * auxiliaryLineLength / 2;
        Vector3 aux2Start = end - perpendicular * auxiliaryLineLength / 2;
        Vector3 aux2End = end + perpendicular * auxiliaryLineLength / 2;

        LineRenderer auxLine1 = GetOrCreateAuxiliaryLine();
        auxLine1.gameObject.SetActive(true); // Bật lên
        auxLine1.SetPosition(0, aux1Start);
        auxLine1.SetPosition(1, aux1End);

        LineRenderer auxLine2 = GetOrCreateAuxiliaryLine();
        auxLine2.gameObject.SetActive(true); // Bật lên
        auxLine2.SetPosition(0, aux2Start);
        auxLine2.SetPosition(1, aux2End);

        // Hiển thị text giữa hai line phụ
        TextMeshPro textMesh = GetOrCreateText();
        textMesh.text = $"{distanceInCm:F1} cm";
        textMesh.transform.position = (aux1End + aux2End) / 2 + Vector3.up * 0.05f; // Đặt text phía trên line chính

        // Xoay text để luôn hướng camera
        textMesh.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    public void UpdateLinesAndDistances(List<GameObject> checkpoints)
    {
        int pointCount = checkpoints.Count;
        if (pointCount < 2) return;

        // Đảm bảo linePool có đủ line
        while (linePool.Count < pointCount)
        {
            GameObject newLine = Instantiate(linePrefab);
            LineRenderer lr = newLine.GetComponent<LineRenderer>();
            linePool.Add(lr);
        }

        // Đảm bảo textPool có đủ text
        while (textPool.Count < pointCount)
        {
            GameObject newText = Instantiate(distanceTextPrefab);
            TextMeshPro tmp = newText.GetComponent<TextMeshPro>();
            textPool.Add(tmp);
        }

        // Cập nhật line và text
        for (int i = 0; i < pointCount; i++)
        {
            int nextIndex = (i + 1) % pointCount;

            // Cập nhật vị trí line
            linePool[i].gameObject.SetActive(true);
            linePool[i].SetPosition(0, checkpoints[i].transform.position);
            linePool[i].SetPosition(1, checkpoints[nextIndex].transform.position);

            // Tính khoảng cách và cập nhật text
            float distanceInCm = Vector3.Distance(checkpoints[i].transform.position, checkpoints[nextIndex].transform.position) * 100f;
            textPool[i].gameObject.SetActive(true);
            textPool[i].text = $"{distanceInCm:F1} cm";
            textPool[i].transform.position = (checkpoints[i].transform.position + checkpoints[nextIndex].transform.position) / 2;

            // Debug kiểm tra
            Debug.Log($"[UpdateLinesAndDistances] Cạnh {i + 1}: {distanceInCm:F1} cm | " +
                        $"Start: {checkpoints[i].transform.position} | End: {checkpoints[nextIndex].transform.position}");
        }

        // Ẩn các line và text dư thừa (nếu có)
        for (int i = pointCount; i < linePool.Count; i++)
        {
            linePool[i].gameObject.SetActive(false);
        }

        for (int i = pointCount; i < textPool.Count; i++)
        {
            textPool[i].gameObject.SetActive(false);
        }
    }

    private LineRenderer GetOrCreateLine()
    {
        foreach (var line in linePool)
        {
            if (!line.gameObject.activeSelf)
            {
                line.gameObject.SetActive(true);
                return line;
            }
        }

        GameObject lineObj = Instantiate(linePrefab);
        lineObj.transform.SetParent(modelRoot); // ← Gắn vào modelRoot
        LineRenderer newLine = lineObj.GetComponent<LineRenderer>();
        linePool.Add(newLine);
        return newLine;
    }
    private LineRenderer GetOrCreateAuxiliaryLine()
    {
        foreach (var auxLine in auxiliaryLinesPool)
        {
            if (!auxLine.gameObject.activeSelf)
            {
                auxLine.gameObject.SetActive(true);
                return auxLine;
            }
        }
        GameObject auxObj = Instantiate(auxiliaryLinePrefab);
        auxObj.transform.SetParent(modelRoot);
        LineRenderer newAuxLine = auxObj.GetComponent<LineRenderer>();

        newAuxLine.gameObject.SetActive(true); // Đảm bảo nó được hiển thị
        auxiliaryLinesPool.Add(newAuxLine);

        return newAuxLine;
    }
    private TextMeshPro GetOrCreateText()
    {
        foreach (var text in textPool)
        {
            if (!text.gameObject.activeSelf)
            {
                text.gameObject.SetActive(true);

                //  Kiểm tra và bật MeshRenderer
                MeshRenderer meshRenderer = text.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.sortingOrder = 50; // Đặt giá trị cao hơn để không bị AR che khuất
                }
                return text;
            }
        }

        GameObject textObj = Instantiate(distanceTextPrefab);
        textObj.transform.SetParent(modelRoot);
        TextMeshPro newText = textObj.GetComponent<TextMeshPro>();

        // Bật MeshRenderer nếu bị tắt
        MeshRenderer textRenderer = newText.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.enabled = true;
            textRenderer.sortingOrder = 50;
        }

        newText.fontSize = 2.5f; // Tăng kích thước chữ
        newText.alignment = TextAlignmentOptions.Center; // Căn giữa
        newText.transform.rotation = Quaternion.Euler(90, 0, 0); // Xoay đúng góc camera

        textPool.Add(newText);
        return newText;
    }
}
