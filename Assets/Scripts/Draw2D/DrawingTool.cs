using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawingTool : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;

    [Header("Materials")]
    public Material dashedMaterial;
    public Material solidMaterial;
    public Material doorMaterial;
    public Material windowMaterial;


    private List<LineRenderer> linePool = new List<LineRenderer>(); // Object Pooling
    private List<TextMeshPro> textPool = new List<TextMeshPro>();

    public List<WallLine> wallLines = new List<WallLine>();
    public LineType currentLineType = LineType.Wall;

    public List<LineRenderer> lines = new List<LineRenderer>();
    public List<TextMeshPro> distanceTexts = new List<TextMeshPro>();

    private LineRenderer previewLine = null; // Dùng cho đường preview
    private TextMeshPro previewText = null; // Dùng cho khoảng cách preview

    private float auxiliaryLineLength = 0.1f; // Độ dài line phụ (10cm)
    private GameObject selectedCheckpoint = null; // Điểm được chọn để di chuyển    

    Material GetMaterialForType(LineType type)
    {
        switch (type)
        {
            case LineType.Wall:
                return solidMaterial;
            case LineType.Door:
                return doorMaterial;
            case LineType.Window:
                return windowMaterial;
            default:
                return solidMaterial;
        }
    }


    public void DrawLineAndDistance(Vector3 start, Vector3 end)
    {
        // GameObject go = Instantiate(linePrefab);
        // LineRenderer lr = go.GetComponent<LineRenderer>();
        LineRenderer lr = GetOrCreateLine(); // ✅ Dùng pool thay vì Instantiate
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Đảm bảo LineRenderer setup chuẩn để tile texture hoạt động tốt
        lr.textureMode = LineTextureMode.Tile;
        lr.alignment = LineAlignment.View; // Quan trọng: để line luôn xoay đúng góc nhìn
        lr.numCapVertices = 0;
        lr.widthMultiplier = 0.1f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Lấy chiều dài đoạn
        float len = Vector3.Distance(start, end);

        // Clone vật liệu để tránh sharedMaterial bug
        Material matInstance;
        if (currentLineType == LineType.Door || currentLineType == LineType.Window)
        {
            matInstance = new Material(GetMaterialForType(currentLineType)); // dashedMaterial phải là Unlit và có WrapMode = Repeat
        }
        else
        {
            matInstance = new Material(GetMaterialForType(currentLineType)); // solid, wall, v.v.
        }

        // Scale texture tile theo chiều dài
        if (matInstance.HasProperty("_MainTex"))
        {
            matInstance.mainTextureScale = new Vector2(len * 2f, 1f); // nhân đôi để tile dày hơn
        }

        // Gán vật liệu
        lr.material = matInstance;

        // Lưu line đã vẽ
        lines.Add(lr);

        // Khoảng cách và text
        float distanceInCm = len * 100f;

        // Tạo line phụ để đặt text (vuông góc line chính)
        Vector3 dir = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.up).normalized;

        Vector3 aux1End = start + perpendicular * auxiliaryLineLength / 2;
        Vector3 aux2End = end + perpendicular * auxiliaryLineLength / 2;

        // Hiển thị text khoảng cách
        // TextMeshPro textMesh = GetOrCreateText();
        // textMesh.text = $"{distanceInCm:F1} cm";
        TextMeshPro textMesh = GetOrCreateText(); // Dùng pool
        textMesh.text = $"{distanceInCm:F1} cm";

        Vector3 textPosition = (aux1End + aux2End) / 2;
        textMesh.transform.position = textPosition;

        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        textMesh.transform.rotation = Quaternion.Euler(90, 0, angle);

        // Lưu dữ liệu tường
        wallLines.Add(new WallLine(start, end, currentLineType));
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

        for (int i = 0; i < pointCount; i++)
        {
            int nextIndex = (i + 1) % pointCount;

            linePool[i].gameObject.SetActive(true);
            linePool[i].SetPosition(0, checkpoints[i].transform.position);
            linePool[i].SetPosition(1, checkpoints[nextIndex].transform.position);

            float distanceInCm = Vector3.Distance(checkpoints[i].transform.position, checkpoints[nextIndex].transform.position) * 100f;
            textPool[i].gameObject.SetActive(true);
            textPool[i].text = $"{distanceInCm:F1} cm";
            textPool[i].transform.position = (checkpoints[i].transform.position + checkpoints[nextIndex].transform.position) / 2;

            // Cập nhật trạng thái line khi đang chọn checkpoint
            if (selectedCheckpoint == checkpoints[i] || selectedCheckpoint == checkpoints[nextIndex])
            {
                linePool[i].startWidth = linePool[i].endWidth = 0.05f; // to hơn khi thao tác
                linePool[i].material.color = Color.blue; // màu khác biệt để dễ nhận diện
            }
            else
            {
                linePool[i].startWidth = linePool[i].endWidth = 0.02f; // mặc định
                linePool[i].material.color = Color.black;
            }

            Debug.Log($"[UpdateLinesAndDistances] Cạnh {i + 1}: {distanceInCm:F1} cm | " +
                        $"Start: {checkpoints[i].transform.position} | End: {checkpoints[nextIndex].transform.position}");
        }

        // Ẩn các line và text dư thừa (nếu có)
        for (int i = pointCount; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);

        for (int i = pointCount; i < textPool.Count; i++)
            textPool[i].gameObject.SetActive(false);
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

        Vector3 textPos = (start + end) / 2 + new Vector3(0, 0.05f, 0); // Đẩy lên cao một chút
        previewText.transform.position = textPos;

        // Xoay text luôn hướng về camera
        if (Camera.main != null)
        {
            Vector3 dir = (end - start).normalized;
            // Xoay text để luôn song song line
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg; // Tính góc từ trục X
            previewText.transform.rotation = Quaternion.Euler(90, 0, angle);
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
    public void ClearAllLines()
    {
        foreach (var lr in linePool)
            lr.gameObject.SetActive(false);

        foreach (var tmp in textPool)
            tmp.gameObject.SetActive(false);

        wallLines.Clear();
    }

    public void ResetLineAppearance()
    {
        foreach (LineRenderer lr in linePool)
        {
            lr.startWidth = lr.endWidth = 0.02f; // kích thước line mặc định
            lr.material.color = Color.black;     // màu sắc mặc định
        }

        foreach (TextMeshPro tmp in textPool)
        {
            tmp.fontSize = 3f; // kích thước font mặc định
            tmp.color = Color.black;
        }
    }
}
