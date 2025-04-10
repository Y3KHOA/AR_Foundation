using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;

    private List<LineRenderer> linePool = new List<LineRenderer>(); // Object Pooling
    private List<TextMeshPro> textPool = new List<TextMeshPro>();

    public List<LineRenderer> lines = new List<LineRenderer>(); // Thêm danh sách này
    public List<TextMeshPro> distanceTexts = new List<TextMeshPro>(); // Thêm danh sách này

    private LineRenderer previewLine = null; // Dùng cho đường preview
    private TextMeshPro previewText = null; // Dùng cho khoảng cách preview

    private string measurementUnit = "m"; // Đơn vị đo lường

    void Start()
    {
        // Lấy đơn vị đã lưu từ MainMenu
        measurementUnit = PlayerPrefs.GetString("SelectedUnit", "m");
        Debug.Log("Don vi nhan duoc: " + measurementUnit);
    }

    public void SetMeasurementUnit(string unit)
    {
        measurementUnit = unit;
        PlayerPrefs.SetString("SelectedUnit", unit);
        PlayerPrefs.Save();
        Debug.Log("Da cap nhat don vi: " + unit);
    }

    public void DrawLineAndDistance(Vector3 start, Vector3 end)
    {
        if (linePrefab == null || distanceTextPrefab == null)
        {
            Debug.LogError("Prefab line hoac distanceTextPrefab chua duoc thiet lap!");
            return;
        }

        LineRenderer line = GetOrCreateLine();
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        lines.Add(line); // Đảm bảo lưu vào danh sách

        float distance = ConvertDistance(Vector3.Distance(start, end));

        // Debug Log để kiểm tra khoảng cách
        Debug.Log($"[DrawLineAndDistance] Đoạn {lines.Count}: {distance:F2} {measurementUnit} | Start: {start} | End: {end}");


        // Lấy hoặc tạo mới TextMeshPro
        TextMeshPro textMesh = GetOrCreateText();
        textMesh.text = $"{distance:F2} {measurementUnit}";
        textMesh.transform.position = (start + end) / 2;

        // Lưu vào cả distanceTexts và textPool để đảm bảo nó không mất
        distanceTexts.Add(textMesh);
        if (!textPool.Contains(textMesh)) textPool.Add(textMesh);
    }

    public void UpdateLinesAndDistances(List<GameObject> checkpoints)
    {
        int pointCount = checkpoints.Count;
        if (pointCount < 2) return;

        measurementUnit = PlayerPrefs.GetString("SelectedUnit", "m");

        for (int i = 0; i < pointCount; i++)
        {
            int nextIndex = (i + 1) % pointCount;

            if (i < linePool.Count)
            {
                linePool[i].SetPosition(0, checkpoints[i].transform.position);
                linePool[i].SetPosition(1, checkpoints[nextIndex].transform.position);
            }

            // float distanceInCm = Vector3.Distance(checkpoints[i].transform.position, checkpoints[nextIndex].transform.position) * 100f;
            float distance = ConvertDistance(Vector3.Distance(checkpoints[i].transform.position, checkpoints[nextIndex].transform.position));

            // Debug Log để kiểm tra cập nhật khoảng cách
            Debug.Log($"[UpdateLinesAndDistances] Canh {i + 1}: {distance:F2} {measurementUnit} | " +
                        $"Start: {checkpoints[i].transform.position} | " +
                        $"End: {checkpoints[nextIndex].transform.position}");

            if (i < textPool.Count)
            {
                textPool[i].text = $"{distance:F2} {measurementUnit}";
                textPool[i].transform.position = (checkpoints[i].transform.position + checkpoints[nextIndex].transform.position) / 2;
            }
        }
    }

    private float ConvertDistance(float distanceInMeters)
    {
        switch (measurementUnit)
        {
            case "cm": return distanceInMeters * 100f;
            case "m": return distanceInMeters * 1f;
            case "feet": return distanceInMeters * 3.28084f;
            case "inch": return distanceInMeters * 39.3701f;
            default: return distanceInMeters;
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

        // float distanceInCm = Vector3.Distance(start, end) * 100f;
        float distance = ConvertDistance(Vector3.Distance(start, end));

        // Kiểm tra xem đã có previewText chưa
        if (previewText == null)
        {
            previewText = Instantiate(distanceTextPrefab).GetComponent<TextMeshPro>();
            previewText.gameObject.name = "PreviewText";

            // Bật MeshRenderer nếu bị tắt
            MeshRenderer textRenderer = previewText.GetComponent<MeshRenderer>();
            if (textRenderer != null) textRenderer.enabled = true;

            previewText.fontSize = 0.5f; // Tăng font size
            previewText.alignment = TextAlignmentOptions.Center;
        }

        // Hiển thị text
        previewText.gameObject.SetActive(true);
        previewText.text = $"{distance:F2} {measurementUnit}";

        // Đặt text lên trên điểm preview (end)
        previewText.transform.position = end + new Vector3(0, 0.1f, 0);
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

        newText.fontSize = 0.5f; // Tăng kích thước chữ
        newText.alignment = TextAlignmentOptions.Center; // Căn giữa

        textPool.Add(newText);
        return newText;
    }
    public void ShowAreaText(Vector3 position, float area)
    {
        if (distanceTextPrefab != null)
        {
            // Chuyển đổi diện tích sang đơn vị đã chọn
            float convertedArea = ConvertDistance(area);

            GameObject textObj = Instantiate(distanceTextPrefab, position, Quaternion.identity);
            TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();

            if (textMesh != null)
            {
                // Hiển thị diện tích với đơn vị đã chuyển đổi
                textMesh.text = $"Dien tich: {convertedArea:F2} {measurementUnit}2";
                textMesh.alignment = TextAlignmentOptions.Center;

                // Hướng mặt chữ về phía camera
                textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - Camera.main.transform.position);
            }
        }
    }
    public void ClearAllLines()
    {
        // Ẩn toàn bộ line đã tạo
        foreach (var line in linePool)
        {
            if (line != null)
                line.gameObject.SetActive(false);
        }

        // Ẩn toàn bộ text đã tạo
        foreach (var text in textPool)
        {
            if (text != null)
                text.gameObject.SetActive(false);
        }

        // Xóa danh sách lines và distanceTexts đang sử dụng
        lines.Clear();
        distanceTexts.Clear();

        // Tắt preview nếu đang hiện
        ClearPreviewLine();

        Debug.Log("Đã reset tất cả line và text hiển thị.");
    }

    public void DestroyPreviewObjects()
    {
        if (previewLine != null)
        {
            Destroy(previewLine.gameObject);
            previewLine = null;
        }

        if (previewText != null)
        {
            Destroy(previewText.gameObject);
            previewText = null;
        }

        Debug.Log("Đã xóa previewLine và previewText hoàn toàn.");
    }
}
