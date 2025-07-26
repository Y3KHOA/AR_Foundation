using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Drawing2D : MonoBehaviour
{
    public float wallTextOffset = 0.2f;
    public float doorTextOffset = 0.2f;
    public float windowTextOffset = 0.2f;
    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject distanceTextPrefab;

    [Header("Parent")]
    public Transform modelRoot;

    [Header("Materials")]
    public Material solidMaterial;
    public Material doorMaterial;
    public Material windowMaterial;

    private List<LineRenderer> linePool = new List<LineRenderer>(); // Object Pooling
    private List<TextMeshPro> textPool = new List<TextMeshPro>();

    public List<WallLine> wallLines = new List<WallLine>();
    public LineType currentLineType = LineType.Wall;

    public List<LineRenderer> lines = new List<LineRenderer>();

    private float auxiliaryLineLength = 0.1f; // Độ dài line phụ (10cm)  

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
        LineRenderer lr = GetOrCreateLine(); // Dùng pool thay vì Instantiate

        // Đảm bảo parent là modelRoot nếu chưa đúng
        if (lr.transform.parent != modelRoot)
            lr.transform.SetParent(modelRoot, false);

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Đảm bảo LineRenderer setup chuẩn để tile texture hoạt động tốt
        lr.textureMode = LineTextureMode.Tile;
        lr.alignment = LineAlignment.View; // Quan trọng: để line luôn xoay đúng góc nhìn
        lr.numCapVertices = 0;
        lr.widthMultiplier = 0.04f;
        lr.positionCount = 2;

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

        matInstance.renderQueue = 3100; // ← Ép vẽ sau grid

        // Gán vật liệu
        lr.material = matInstance;
        // Ưu tiên nét đứt (cửa/cửa sổ) vẽ sau
        if (currentLineType == LineType.Door || currentLineType == LineType.Window)
            lr.sortingOrder = 20;
        else
            lr.sortingOrder = 10;

        // Lưu line đã vẽ
        lines.Add(lr);

        // Khoảng cách và text
        float distanceInM = len * 1f;

        // Tạo line phụ để đặt text (vuông góc line chính)
        Vector3 dir = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.up).normalized;

        Vector3 aux1End = start + perpendicular * auxiliaryLineLength / 2;
        Vector3 aux2End = end + perpendicular * auxiliaryLineLength / 2;

        TextMeshPro textMesh = GetOrCreateText(); // Dùng pool

        // Đảm bảo parent là modelRoot nếu chưa đúng
        if (textMesh.transform.parent != modelRoot)
            textMesh.transform.SetParent(modelRoot, false);

        textMesh.text = $"{distanceInM:F2} m";

        Vector3 textPosition = (aux1End + aux2End) / 2;

        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        textMesh.transform.rotation = Quaternion.Euler(90, 0, angle);
        textMesh.transform.position = textPosition + textMesh.transform.up * GetTextOffset(currentLineType);
        textMesh.color = GetTextColor(currentLineType);
        // Lưu dữ liệu tường
        wallLines.Add(new WallLine(start, end, currentLineType));
    }

    private float GetTextOffset(LineType lineType)
    {
        switch (lineType)
        {
            case LineType.Wall:
                return wallTextOffset;
            case LineType.Door:
                return doorTextOffset;
            case LineType.Window:
                return windowTextOffset;
            default:
                throw new ArgumentOutOfRangeException(nameof(lineType), lineType, null);
        }
    }

    private Color GetTextColor(LineType lineType)
    {
        switch (lineType)
        {
            case LineType.Wall:
                return Color.black;
            case LineType.Door:
                return Color.red;
            case LineType.Window:
                return Color.blue;
            default:
                throw new ArgumentOutOfRangeException(nameof(lineType), lineType, null);
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
        LineRenderer newLine = lineObj.GetComponent<LineRenderer>();

        // Gán parent và layer
        if (modelRoot != null)
            lineObj.transform.SetParent(modelRoot, false);
        SetLayerRecursively(lineObj, modelRoot.gameObject.layer); 

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

        if (modelRoot != null)
            textObj.transform.SetParent(modelRoot, false);
        SetLayerRecursively(textObj, modelRoot.gameObject.layer);

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

    // === add layer cho các object trong modelRoot
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}