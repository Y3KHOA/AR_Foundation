using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawingTool : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;
    public GameObject auxiliaryLinePrefab; // Line phụ

    private List<LineRenderer> linePool = new List<LineRenderer>(); // Object Pooling
    private List<TextMeshPro> textPool = new List<TextMeshPro>();
    private List<LineRenderer> auxiliaryLinesPool = new List<LineRenderer>(); // Pool line phụ

    public List<LineRenderer> lines = new List<LineRenderer>(); // Thêm danh sách này
    public List<TextMeshPro> distanceTexts = new List<TextMeshPro>(); // Thêm danh sách này

    private LineRenderer previewLine = null; // Dùng cho đường preview
    private TextMeshPro previewText = null; // Dùng cho khoảng cách preview

    private float auxiliaryLineLength = 0.1f; // Độ dài line phụ (10cm)

    public void DrawLineAndDistance(Vector3 start, Vector3 end)
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

        for (int i = 0; i < pointCount; i++)
        {
            int nextIndex = (i + 1) % pointCount;

            if (i < linePool.Count)
            {
                linePool[i].SetPosition(0, checkpoints[i].transform.position);
                linePool[i].SetPosition(1, checkpoints[nextIndex].transform.position);
            }

            float distanceInCm = Vector3.Distance(checkpoints[i].transform.position, checkpoints[nextIndex].transform.position) * 100f;

            // Debug Log để kiểm tra cập nhật khoảng cách
            Debug.Log($"[UpdateLinesAndDistances] Canh {i + 1}: {distanceInCm:F1} cm | " +
                      $"Start: {checkpoints[i].transform.position} | " +
                      $"End: {checkpoints[nextIndex].transform.position}");

            if (i < textPool.Count)
            {
                textPool[i].text = $"{distanceInCm:F1} cm";
                textPool[i].transform.position = (checkpoints[i].transform.position + checkpoints[nextIndex].transform.position) / 2;
            }
        }
    }

    public void DrawPreviewLine(Vector3 start, Vector3 end)
    {
        if (linePrefab == null || distanceTextPrefab == null)
        {
            Debug.LogError("linePrefab hoac distanceTextPrefab chua duoc thiet lap!");
            return;
        }

        if (Vector3.Distance(start, end) < 0.001f) // Tránh vẽ đoạn quá nhỏ
        {
            Debug.LogWarning("khoang cach qua nho, khong ve duoc!");
            return;
        }

        // Kiểm tra xem đã có previewLine chưa
        if (previewLine == null)
        {
            previewLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
            previewLine.gameObject.name = "PreviewLine";
        }

        // Hiển thị line
        previewLine.gameObject.SetActive(true);
        previewLine.SetPosition(0, start);
        previewLine.SetPosition(1, end);

        float distanceInCm = Vector3.Distance(start, end) * 100f;

        // Kiểm tra xem đã có previewText chưa
        if (previewText == null)
        {
            previewText = Instantiate(distanceTextPrefab).GetComponent<TextMeshPro>();
            previewText.gameObject.name = "PreviewText";

            // Bật MeshRenderer nếu bị tắt
            MeshRenderer textRenderer = previewText.GetComponent<MeshRenderer>();
            if (textRenderer != null) textRenderer.enabled = true;

            previewText.fontSize = 2.5f; // Tăng font size
            previewText.alignment = TextAlignmentOptions.Center;
        }

        // Hiển thị text
        previewText.gameObject.SetActive(true);
        previewText.text = $"{distanceInCm:F1} cm";

        // Đặt text lên trên điểm preview (end)
        // previewText.transform.position = end + new Vector3(0, 0.1f, 0);
        Vector3 textPos = (start + end) / 2 + new Vector3(0, 0.05f, 0); // Đẩy lên cao một chút
        previewText.transform.position = textPos;

        // Xoay text luôn hướng về camera
        if (Camera.main != null)
        {
            previewText.transform.rotation = Quaternion.LookRotation(previewText.transform.position - Camera.main.transform.position);
        }
    }

    public void ClearPreviewLine()
    {
        if (previewLine != null)
            previewLine.gameObject.SetActive(false);

        if (previewText != null)
            previewText.gameObject.SetActive(false);
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
