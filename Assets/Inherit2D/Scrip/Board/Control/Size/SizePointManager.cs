using UnityEngine;
using System.Collections.Generic;
using static Shared;
using Unity.VisualScripting;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Net;
using NUnit.Framework.Internal;
using UnityEngine.U2D;

/// <summary>
/// Lớp SizePointManager dùng để quản lý các điểm kích thước (size points) trong một mô hình 2D, bao gồm việc tạo, cập nhật và hiển thị các điểm góc và trung điểm của một hình đa giác (thường là hình chữ nhật hoặc tường). Nó cũng xử lý việc vẽ đường viền, tính toán diện tích, hiển thị thông tin cạnh và thêm collider cho các đoạn thẳng.
/// </summary>
public class SizePointManager : MonoBehaviour
{
    [Header("Size")]
    public GameObject sizePointPrefab;
    [HideInInspector] public List<SizePointEditor> sizePointList = new List<SizePointEditor>();
    public LineRenderer lineRenderer;
    public Item item;
    public Material backgroundMaterial;
    public Material itemMaterial;
    public Material tempMaterial;
    [HideInInspector] public Material backgroundMaterialTemp;
    private Material itemMaterialTemp;
    public Texture2D backgroundTexture;

    [Header("Text")]
    public TextMeshProUGUI areaText;
    public RectTransform areaTextRect;
    public Font edgeLengthTextFont;

    [Header("Edge")]
    public List<LineRenderer> edgeLineRenderers = new List<LineRenderer>();
    public GameObject edgeLineRendererPrefab;
    public Material edgeLineMaterial;
    public Sprite circleSprite;

    [Header("Parents")]
    public GameObject sizePointParent;
    public GameObject textParent;
    public GameObject colliderParent;
    public GameObject edgeLineParent;

    private const string kindGroundString = "Kết cấu";
    private GameManager gameManager;
    [HideInInspector] public MeshRenderer backgroundMeshRenderer;
    [HideInInspector] public MeshFilter backgroundMeshFilter;
    [HideInInspector] public List<GameObject> edgeLengthTextObjects = new List<GameObject>();
    private List<GameObject> iconObjects = new List<GameObject>();
    private List<GameObject> extensionLineList = new List<GameObject>();
    [HideInInspector] public bool isUsingImageBackground = false;
    [HideInInspector] public List<Vector3> oldSizePointPos = new List<Vector3>();

    //Width
    private float groundWidth = 0.8f;
    private float itemWidth = 0.4f;
    private float offsetDistance = -3.2f;

    public LineType currentLineType = LineType.Wall;
    public List<WallLine> wallLines = new List<WallLine>();
    public List<Room> rooms = new List<Room>();

    //
    private float verticalOffset = -1f;
    private float inwardOffset = 1f;
    private Room currentRoom;

    public void Start()
    {
        currentRoom = new Room();
        RoomStorage.rooms.Add(currentRoom);

        gameManager = GameManager.instance;

        backgroundMaterialTemp = new Material(tempMaterial);
        itemMaterialTemp = new Material(tempMaterial);

        if (item.CompareKindOfItem(kindGroundString))
        {
            lineRenderer.widthMultiplier = groundWidth;
        }
        else
        {
            lineRenderer.widthMultiplier = itemWidth;
        }

        UpdateLineRenderer();
    }

    /// <summary>
    ///  dùng để cập nhật LineRenderer biểu diễn một hình chữ nhật khép kín từ danh sách các điểm sizePointList, đồng thời xử lý thông tin hiển thị như kích thước, diện tích, đường viền, và collider tùy theo loại đối tượng (item). Dưới đây là phân tích chi tiết từng phần:
    /// </summary>
    public void UpdateLineRenderer()
    {
        if (lineRenderer == null || sizePointList.Count <= 0) return;

        lineRenderer.positionCount = sizePointList.Count + 1;  // Số lượng điểm là số sizePoint + 1 cho điểm đầu tiên

        for (int i = 0; i < sizePointList.Count; i++)
        {
            Vector3 position = sizePointList[i].transform.position;
            position.z = 0;

            // Nếu LineRenderer không sử dụng world space, chuyển đổi vị trí sang local space
            if (!lineRenderer.useWorldSpace)
            {
                position = lineRenderer.transform.InverseTransformPoint(position);
                position.z = 0;
            }

            lineRenderer.SetPosition(i, position);
        }

        // Gán điểm đầu vào vị trí cuối để khép kín
        Vector3 firstPosition = sizePointList[0].transform.position;
        if (!lineRenderer.useWorldSpace)
        {
            firstPosition = lineRenderer.transform.InverseTransformPoint(firstPosition);
            firstPosition.z = 0;
        }
        lineRenderer.SetPosition(sizePointList.Count, firstPosition);

        // Tạo các điểm cho viền hình chữ nhật
        Vector3[] corners = new Vector3[5];
        corners[0] = lineRenderer.GetPosition(0);
        corners[1] = lineRenderer.GetPosition(2);
        corners[2] = lineRenderer.GetPosition(4);
        corners[3] = lineRenderer.GetPosition(6);
        corners[4] = corners[0];

        //Nếu không phải "ground" thì tính chiều dài và chiều rộng
        if (!item.CompareKindOfItem(kindGroundString))
        {
            item.length = (float)Math.Round(Vector3.Distance(corners[0], corners[1]) / 10, 2);
            item.width = (float)Math.Round(Vector3.Distance(corners[1], corners[2]) / 10, 2);

            gameManager.guiCanvasManager.infomationItemCanvas.UpdateInfomation(item);
        }
        //Nếu là "ground" thì có logic bị comment (có thể dùng sau)
        else
        {
            //float c1 = (float)Math.Round(Vector3.Distance(corners[0], corners[1]) / 10, 2);
            //float c2 = (float)Math.Round(Vector3.Distance(corners[1], corners[2]) / 10, 2);
            //float c3 = (float)Math.Round(Vector3.Distance(corners[2], corners[3]) / 10, 2);
            //float c4 = (float)Math.Round(Vector3.Distance(corners[3], corners[4]) / 10, 2);

            //item.edgeLengthList[0] = c1;
            //item.edgeLengthList[1] = c2;
            //item.edgeLengthList[2] = c3;
            //item.edgeLengthList[3] = c4;

            //item.width = c1;
            //item.height = c2;

            //if (isUsingImageBackground)
            //{
            //    float avg = (c1 + c2 + c3 + c4) / 4;
            //    backgroundMaterialTemp.SetVector("_Tiling", new Vector4(avg / 1.2f, avg / 1.2f, 0, 0));
            //}
        }

        // Vẽ nền và các thành phần phụ
        CreateBackgroundMesh(corners);
        UpdateAreaText();
        DrawEdgeLengthText(corners);
        DrawEdgeLines(corners);
        if (item.CompareKindOfItem(kindGroundString)) AddLineColliders();
    }

    /// <summary>
    /// Hàm UpdateMidPointsFromCorners() có mục đích tự động cập nhật vị trí các điểm giữa (midpoints) dựa trên vị trí các điểm góc (corners) trong danh sách sizePointList, theo cấu trúc hình chữ nhật (hoặc đa giác 4 cạnh mở rộng thành 8 điểm).
    /// </summary>
    private void UpdateMidPointsFromCorners()
    {
        //Vòng lặp chỉ đi qua các điểm "lẻ"
        for (int i = 1; i < sizePointList.Count; i += 2)
        {
            //Tìm hai điểm góc gần kề với điểm giữa tại i
            int cornerA = (i - 1 + 8) % 8;
            int cornerB = (i + 1) % 8;

            //Tính trung điểm và cập nhật lại vị trí
            Vector3 midpoint = (sizePointList[cornerA].transform.position + sizePointList[cornerB].transform.position) / 2;
            sizePointList[i].transform.position = midpoint;
        }
    }

    /// <summary>
    /// Hàm UpdateCornersFromMidpoint(int index, Vector3 newPosition) có chức năng cập nhật lại vị trí hai điểm góc gần kề dựa trên vị trí mới của một điểm giữa (midpoint), nhằm giữ nguyên hình dạng hình học đối xứng.
    /// </summary>
    private void UpdateCornersFromMidpoint(int index, Vector3 newPosition)
    {
        if (index < 0 || index > 7 || index % 2 != 1) return;

        //Xác định hai điểm góc gần điểm giữa
        int cornerA = (index - 1 + 8) % 8;
        int cornerB = (index + 1) % 8;

        //Tính độ lệch vị trí của midpoint
        Vector3 midpointOld = (sizePointList[cornerA].transform.position + sizePointList[cornerB].transform.position) / 2;
        Vector3 offset = newPosition - midpointOld;

        //Di chuyển hai góc theo cùng offset
        sizePointList[cornerA].transform.position += offset;
        sizePointList[cornerB].transform.position += offset;
    }

    /// <summary>
    /// Hàm MoveSizePoint(int index, Vector3 newPosition) là một phương thức điều khiển di chuyển điểm kích thước (size point) trong một hình học (thường là hình chữ nhật hoặc vùng sàn), và tự động cập nhật lại hình dạng và hiển thị sau đó.
    /// </summary>
    public void MoveSizePoint(int index, Vector3 newPosition)
    {
        if (index < 0 || index >= sizePointList.Count) return;

        //Cập nhật vị trí mới cho điểm được chọn
        sizePointList[index].transform.position = newPosition;

        //Phân biệt loại điểm để xử lý phù hợp
        // if (sizePointList[index].pointType == SizePointType.Corner)
        // {
        //     UpdateMidPointsFromCorners();
        // }
        // else if (sizePointList[index].pointType == SizePointType.Midpoint)
        // {
        //     UpdateCornersFromMidpoint(index, newPosition);
        //     UpdateMidPointsFromCorners();
        // }

        //Vẽ lại hình   
        UpdateLineRenderer();

        //Đảm bảo tất cả các điểm nằm đúng trên mặt phẳng Z = -4
        for (int i = 0; i < sizePointList.Count; i++)
        {
            sizePointList[i].transform.position = new Vector3(sizePointList[i].transform.position.x, sizePointList[i].transform.position.y, -4f);
        }

    }

    public List<SizePointEditor> GetSizePoints()
    {
        return sizePointList;
    }

    /// <summary>
    /// Hàm DrawOutline(Item item) dùng để vẽ viền hình chữ nhật (outline) của một đối tượng Item bằng cách sử dụng LineRenderer. Nó cũng kết hợp hiển thị thông tin như cạnh, nền, và collider khi cần.
    /// </summary>
    public void DrawOutline(Item item)
    {
        if (lineRenderer == null) return;

        // Tạo các điểm cho viền hình chữ nhật
        Vector3[] corners = new Vector3[5]; // 5 điểm vì vòng lặp quay lại điểm đầu tiên

        if (item.CompareKindOfItem(kindGroundString))
        {
            // Kiểm tra danh sách độ dài cạnh có đủ 4 cạnh không
            if (item.edgeLengthList == null || item.edgeLengthList.Count < 4)
            {
                Debug.LogError($"[DrawOutline] edgeLengthList ko du! Count = {item.edgeLengthList?.Count ?? -1}");
                return;
            }

            // Phân nhánh theo loại item
            float c1 = item.edgeLengthList[0] * 10f; // top
            float c2 = item.edgeLengthList[1] * 10f; // right
            float c3 = item.edgeLengthList[2] * 10f; // bottom
            float c4 = item.edgeLengthList[3] * 10f; // left

            // Đặt góc theo cạnh tương ứng
            corners[0] = new Vector3(-c4 / 2, c1 / 2, 0);  // Góc trên bên tri
            corners[1] = new Vector3(c2 / 2, c1 / 2, 0);   // Góc trên bên phải
            corners[2] = new Vector3(c2 / 2, -c3 / 2, 0);  // Góc dưới bên phải
            corners[3] = new Vector3(-c4 / 2, -c3 / 2, 0); // Góc dưới bên trái
            corners[4] = corners[0];  // Quay lại điểm đầu tiên để tạo vòng tròn
        }
        else
        {
            // Lấy kích thước của item
            float width = item.width * 10f;
            float length = item.length * 10f;

            // Góc tính theo chiều dài và rộng
            corners[0] = new Vector3(-length / 2, width / 2, 0);  // Góc trên bên trái
            corners[1] = new Vector3(length / 2, width / 2, 0);   // Góc trên bên phải
            corners[2] = new Vector3(length / 2, -width / 2, 0);  // Góc dưới bên phải
            corners[3] = new Vector3(-length / 2, -width / 2, 0); // Góc dưới bên trái
            corners[4] = corners[0];  // Quay lại điểm đầu tiên để tạo vòng tròn
        }

        // Vẽ hình bằng LineRenderer
        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);

        //Gọi các hàm bổ trợ để hoàn thiện hiển thị
        CreateBackgroundMesh(corners);
        DrawEdgeLengthText(corners);
        DrawEdgeLines(corners);
        if (item.CompareKindOfItem(kindGroundString)) AddLineColliders();
    }

    /// <summary>
    /// Hàm DrawOutline(List<Vector3> vector3s) được thiết kế để vẽ đường viền (outline) từ danh sách điểm bất kỳ (thường là các đỉnh của đa giác) bằng LineRenderer. Khác với hàm trước đó (vẽ hình chữ nhật), hàm này linh hoạt hơn vì hoạt động với danh sách điểm tùy ý – phù hợp cho hình đa giác.
    /// </summary>
    public void DrawOutline(List<Vector3> vector3s)
    {
        if (lineRenderer == null || vector3s == null || vector3s.Count < 2) return;

        // Thêm điểm đầu vào cuối danh sách để khép kín đường vẽ
        //Tạo vòng khép kín từ danh sách điểm
        List<Vector3> closedLoop = new List<Vector3>(vector3s);
        closedLoop.Add(vector3s[0]);

        //Cập nhật LineRenderer để vẽ outline
        lineRenderer.positionCount = closedLoop.Count;
        lineRenderer.SetPositions(closedLoop.ToArray());

        //Gọi các hàm phụ để hoàn thiện hiển thị
        CreateBackgroundMesh(closedLoop.ToArray());
        DrawEdgeLengthText(closedLoop.ToArray());
        DrawEdgeLines(closedLoop.ToArray());

        //Thêm collider nếu là item loại kindGroundString
        if (item.CompareKindOfItem(kindGroundString))
        {
            AddLineColliders();
        }
    }

    /// <summary>
    /// Hàm DrawEdgeLengthText(Vector3[] corners) dùng để hiển thị chiều dài của các cạnh (giữa các đỉnh corners) bằng cách đặt các TextMesh vào trung điểm của mỗi cạnh, với hướng và khoảng cách hợp lý để người dùng dễ quan sát.
    /// </summary>
    private void DrawEdgeLengthText(Vector3[] corners)
    {
        //Tính số cạnh của hình
        int edgeCount = corners.Length - 1;

        //Tạo thêm TextMesh nếu chưa đủ số lượng
        while (edgeLengthTextObjects.Count < edgeCount)
        {
            GameObject textObject = new GameObject("EdgeLengthText");
            textObject.transform.SetParent(textParent.transform);
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.font = edgeLengthTextFont;
            textMesh.GetComponent<MeshRenderer>().material = edgeLengthTextFont.material;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            edgeLengthTextObjects.Add(textObject);
        }

        //Duyệt từng cạnh để tính chiều dài và hiển thị
        for (int i = 0; i < edgeCount; i++)
        {
            float length = Vector3.Distance(corners[i], corners[(i + 1) % edgeCount]);

            // Tìm trung điểm của cạnh
            Vector3 midpoint = (corners[i] + corners[(i + 1) % edgeCount]) / 2;
            midpoint = transform.TransformPoint(midpoint); // Chuyển sang world space

            // Tính hướng và pháp tuyến của cạnh
            Vector3 edgeDirection = (corners[(i + 1) % edgeCount] - corners[i]).normalized;
            Vector3 normal = transform.TransformDirection(new Vector3(-edgeDirection.y, edgeDirection.x, 0)).normalized;

            // Điều chỉnh khoảng cách text
            float textOffset = Mathf.Max(0.03f * length, 0.5f);
            midpoint += normal * textOffset;
            midpoint.z = -1;

            // Cập nhật vị trí text
            GameObject textObject = edgeLengthTextObjects[i];
            textObject.transform.position = midpoint;

            // Cập nhật nội dung text
            TextMesh textMesh = textObject.GetComponent<TextMesh>();
            textMesh.text = (length / 10).ToString("F2");
            textMesh.fontSize = Mathf.Clamp((int)(length * 3), 5, 10);
            textMesh.color = Color.black;
        }
    }

    /// <summary>
    /// Hàm DrawEdgeLines(Vector3[] corners) có nhiệm vụ vẽ các cạnh riêng biệt của đa giác (không trùng với line chính), đồng thời gắn thêm icon đầu/cuối cạnh và đường mở rộng phụ để hỗ trợ trực quan hóa hoặc tương tác thêm (ví dụ kéo dài, hiệu chỉnh cạnh...).
    /// </summary>
    private void DrawEdgeLines(Vector3[] corners)
    {
        int edgeCount = corners.Length - 1; // Số cạnh của đa giác
        Debug.Log("edgeCount: " + edgeCount);

        //Khởi tạo đủ số lượng LineRenderer cho mỗi cạnh
        while (edgeLineRenderers.Count < edgeCount)
        {
            GameObject edgeLineObject = Instantiate(edgeLineRendererPrefab, edgeLineParent.transform);
            LineRenderer edgeLine = edgeLineObject.GetComponent<LineRenderer>();
            edgeLineRenderers.Add(edgeLine);
        }
        Debug.Log("edgeLineRenderers.Count: " + edgeLineRenderers.Count);

        //Tạo đủ icon và đường mở rộng
        while (iconObjects.Count < edgeCount * 2)
        {
            iconObjects.Add(CreateCircleIcon());
            extensionLineList.Add(CreateExtensionLine());
        }
        // Reset Room data tạm thời
        if (currentRoom == null)
        {
            currentRoom = new Room();
        }
        currentRoom.checkpoints.Clear();
        currentRoom.wallLines.Clear();

        // Vẽ tất cả các cạnh của đa giác
        for (int i = 0; i < edgeCount; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % edgeCount];

            // Tính vector hướng & pháp tuyến
            Vector3 edgeDirection = (end - start).normalized;
            Vector3 normal = Vector3.Cross(edgeDirection, Vector3.forward).normalized;

            // Điều chỉnh khoảng cách offset
            Vector3 offset = normal * offsetDistance;
            Vector3 newStart = start + offset;
            Vector3 newEnd = end + offset;

            // Cập nhật LineRenderer
            LineRenderer edgeLine = edgeLineRenderers[i];
            edgeLine.SetPosition(0, newStart);
            edgeLine.SetPosition(1, newEnd);

            // Cập nhật vị trí icon
            Vector3 adjustedStart = newStart - edgeDirection * -inwardOffset;
            Vector3 adjustedEnd = newEnd - edgeDirection * inwardOffset;

            iconObjects[i * 2].transform.localPosition = adjustedStart;
            iconObjects[i * 2 + 1].transform.localPosition = adjustedEnd;

            // Vị trí & xoay đường mở rộng
            Vector3 avgStart = (start + newStart) / 2f;
            Vector3 avgEnd = (end + newEnd) / 2f;

            Vector3 adjustedAvgStart = avgStart + normal * verticalOffset;
            Vector3 adjustedAvgEnd = avgEnd + normal * verticalOffset;

            extensionLineList[i * 2].transform.localPosition = adjustedAvgStart;
            extensionLineList[i * 2 + 1].transform.localPosition = adjustedAvgEnd;

            // Tính góc quay cho icon và đường mở rộng
            float angle = Mathf.Atan2(edgeDirection.y, edgeDirection.x) * Mathf.Rad2Deg;

            //Dùng hướng cạnh để tính góc xoay, đảm bảo icon và đường mở rộng quay đúng theo hướng cạnh.
            iconObjects[i * 2].transform.rotation = edgeLineParent.transform.rotation * Quaternion.AngleAxis(angle + 180, Vector3.forward);
            iconObjects[i * 2 + 1].transform.rotation = edgeLineParent.transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward);

            extensionLineList[i * 2].transform.rotation = edgeLineParent.transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward);
            extensionLineList[i * 2 + 1].transform.rotation = edgeLineParent.transform.rotation * Quaternion.AngleAxis(angle + 180, Vector3.forward);

            // Thêm đoạn tường
            //         WallLine wall = new WallLine(adjustedStart, adjustedEnd, currentLineType);
            //         wallLines.Add(wall);
            //         newRoom.wallLines.Add(wall);
            //     }

            //         // Lưu checkpoint từ corners
            // foreach (Vector3 corner in corners)
            // {
            //     newRoom.checkpoints.Add(new Vector2(corner.x, corner.y));
            // }
            // Thêm đoạn tường
            // WallLine wall = new WallLine(start, end, currentLineType);
            // wallLines.Add(wall);
            // newRoom.wallLines.Add(wall);
        }

        // Lưu checkpoint từ corners
        foreach (GameObject corner in iconObjects)
        {
            Vector3 pos = corner.transform.position;
            currentRoom.checkpoints.Add(new Vector2(pos.x, pos.y));
        }
        // Tạo wallLines từ checkpoint
        for (int i = 0; i < iconObjects.Count; i++)
        {
            Vector3 p1 = iconObjects[i].transform.position;
            Vector3 p2 = (i == iconObjects.Count - 1)
                ? iconObjects[0].transform.position
                : iconObjects[i + 1].transform.position;

            WallLine wall = new WallLine(p1, p2, currentLineType);
            currentRoom.wallLines.Add(wall);
        }

        Debug.Log("Room saved with " + currentRoom.checkpoints.Count + " points and " + currentRoom.wallLines.Count + " lines.");

        // Lưu vào RoomStorage
        if (!RoomStorage.rooms.Contains(currentRoom))
        {
            RoomStorage.rooms.Clear(); // chỉ cần clear 1 lần đầu
            RoomStorage.rooms.Add(currentRoom);
        }
    }

    /// <summary>
    /// Đoạn code CreateCircleIcon() này có nhiệm vụ tạo một icon hình tròn (thường dùng để đánh dấu đầu mút hoặc midpoint của cạnh trong bản vẽ 2D/3D như CAD).
    /// </summary>
    private GameObject CreateCircleIcon()
    {
        GameObject icon = new GameObject("EdgeIcon");
        icon.transform.SetParent(edgeLineParent.transform);

        Image image = icon.AddComponent<Image>();
        image.sprite = circleSprite;
        image.transform.localScale = Vector3.one * 0.02f;

        return icon;
    }

    /// <summary>
    /// Đoạn code CreateExtensionLine() có nhiệm vụ tạo một đoạn thẳng nhỏ (line) dưới dạng UI Image dùng để hiển thị các đường phụ (extension lines)
    /// </summary>
    private GameObject CreateExtensionLine()
    {
        GameObject icon = new GameObject("ExtensionLine");
        icon.transform.SetParent(edgeLineParent.transform);

        Image image = icon.AddComponent<Image>();
        image.color = Color.black;
        image.transform.localScale = new Vector3(0.003f, 0.055f, 1);

        return icon;
    }

    /// <summary>
    /// Hàm UpdateAreaText() có nhiệm vụ cập nhật thông tin và hiển thị diện tích sàn (floor area) của một đa giác (thường là một căn phòng hoặc mặt sàn) trên UI trong Unity. Nó xử lý việc tính toán diện tích, hiển thị text giữa vùng đã chọn và cập nhật kích thước chữ cho phù hợp.
    /// </summary>
    public void UpdateAreaText()
    {
        if (!areaText.gameObject.activeSelf) return;

        // Tính diện tích
        List<Vector3> points = sizePointList.Select(p => p.transform.position).ToList();
        float area = CalculatePolygonArea(points) / 100f; // tùy đơn vị bạn dùng là cm hay m

        // Gán text
        areaText.text = area.ToString("F2") + "m²";

        // Gán vào panel thông tin bên phải
        // gameManager.guiCanvasManager.infomationItemCanvas.floorAreaText.text = area.ToString("F2") + "m²";
        InfomationItemCanvas.instance.floorAreaText.text = area.ToString("F2") + "m²";

        // Cập nhật vị trí (trung tâm polygon)
        Vector3 center = Vector3.zero;
        foreach (var p in sizePointList)
            center += p.transform.position;
        center /= sizePointList.Count;
        center.z = -1f; // để không bị che hoặc z-fighting

        areaText.transform.position = center;

        // Cập nhật scale cho phù hợp
        float fontSize = Mathf.Clamp(area / 2f, 0.8f, 1f);
        areaTextRect.localScale = new Vector2(fontSize, fontSize);
    }

    /// <summary>
    /// Hàm CalculatePolygonArea(List<Vector3> points) dùng để tính diện tích của một đa giác (polygon) trong không gian 2D (trên mặt phẳng X-Y) từ danh sách các điểm 3D (Vector3), sử dụng công thức hình học Gauss / Shoelace formula.
    /// </summary>
    private float CalculatePolygonArea(List<Vector3> points)
    {
        int n = points.Count;
        float area = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 p1 = transform.InverseTransformPoint(points[i]);
            Vector3 p2 = transform.InverseTransformPoint(points[(i + 1) % n]);

            area += (p1.x * p2.y) - (p2.x * p1.y);
        }

        return Mathf.Abs(area) / 2f;
    }

    /// <summary>
    /// Hàm CreateSizePoints() có nhiệm vụ tạo các điểm đo (SizePoint) bao gồm:
    /// góc(Corner) : tại các điểm đầu mút của đoạn tường.
    /// trung điểm (Midpoint): tại giữa mỗi đoạn tường.
    /// Dùng để hiển thị hoặc chỉnh sửa các cạnh trong một mô hình (thường là tường hoặc sàn) từ LineRenderer.
    /// </summary>
    public void CreateSizePoints()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2)
        {
            Debug.LogWarning("LineRenderer không hợp lệ hoặc không đủ điểm.");
            return;
        }

        if (sizePointList.Count != 0)
        {
            foreach (SizePointEditor go in sizePointList)
            {
                Destroy(go.gameObject);
            }
            sizePointList.Clear();
        }

        int numPoints = lineRenderer.positionCount;

        for (int i = 0; i < numPoints - 1; i++)
        {
            //Trung điểm
            Vector3 pointA = lineRenderer.GetPosition(i);
            Vector3 pointB = lineRenderer.GetPosition(i + 1);

            if (!lineRenderer.useWorldSpace)
            {
                pointA = lineRenderer.transform.TransformPoint(pointA);
                pointB = lineRenderer.transform.TransformPoint(pointB);
            }

            Vector3 midpoint = (pointA + pointB) / 2;

            //Góc
            CreateSizePoint(pointA, SizePointType.Corner);

            //Trung điểm
            CreateSizePoint(midpoint, SizePointType.Midpoint);
        }

        for (int i = 0; i < sizePointList.Count; i++)
        {
            sizePointList[i].index = i;

            if (!item.CompareKindOfItem(kindGroundString))
            {
                if (i % 2 == 0)
                    sizePointList[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hàm CreateSizePoint(Vector3 position, SizePointType pointType) dùng để tạo và cấu hình một điểm đo kích thước (size point) tại vị trí xác định, rồi thêm nó vào danh sách quản lý. Đây là một phần trong hệ thống đo đạc (góc/trung điểm) trong bản vẽ tường hoặc sàn.
    /// </summary>
    private void CreateSizePoint(Vector3 position, SizePointType pointType)
    {
        SizePointEditor sizePoint = Instantiate(sizePointPrefab, sizePointParent.transform).GetComponent<SizePointEditor>();
        sizePoint.transform.position = position;
        sizePoint.sizePointManager = this;
        sizePoint.pointType = pointType;
        sizePointList.Add(sizePoint);
    }

    /// <summary>
    /// Hàm AddMeshCollider() dùng để gắn (hoặc đảm bảo đã gắn) một MeshCollider vào GameObject hiện tại và đồng bộ lại hình dạng collider theo MeshFilter.
    /// </summary>
    private void AddMeshCollider()
    {
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();

        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = GetComponent<MeshFilter>().mesh;
        meshCollider.convex = false; // Nếu muốn va chạm chính xác với Mesh
    }

    /// <summary>
    /// Hàm AddLineColliders() có nhiệm vụ tạo collider cho từng đoạn thẳng trong LineRenderer để phục vụ mục đích va chạm, tương tác (như raycast, chọn tường, xóa tường...).
    /// </summary>
    private void AddLineColliders()
    {
        // Xóa các collider cũ
        foreach (Transform child in colliderParent.transform)
        {
            if (child.name == "LineSegmentCollider")
            {
                Destroy(child.gameObject);
            }
        }

        // Tạo collider cho từng đoạn thẳng
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end = lineRenderer.GetPosition(i + 1);

            // Chuyển đổi vị trí sang world space nếu cần
            if (!lineRenderer.useWorldSpace)
            {
                start = lineRenderer.transform.TransformPoint(start);
                end = lineRenderer.transform.TransformPoint(end);
            }

            // Tính toán trung điểm và hướng
            Vector3 midPoint = (start + end) / 2;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            GameObject colliderObject = new GameObject("LineSegmentCollider");
            colliderObject.AddComponent<Wall>().index = i;
            colliderObject.tag = "Wall";
            colliderObject.transform.position = midPoint;

            // Xoay thêm 90 độ quanh trục Z
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            Quaternion additionalRotation = Quaternion.Euler(0, 0, 90); // Xoay thêm 90 độ quanh trục Z
            colliderObject.transform.rotation = rotation * additionalRotation; // Áp dụng phép quay bổ sung

            // Đặt colliderObject vào làm con của đối tượng chính
            colliderObject.transform.SetParent(colliderParent.transform);

            // Thêm BoxCollider
            BoxCollider boxCollider = colliderObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(distance, 0.8f, 0.1f);
        }
    }

    //public void CreateBackgroundMeshh(Vector3[] corners)
    //{
    //    if (corners == null || corners.Length < 3)
    //    {
    //        Debug.LogError("Không đủ điểm để tạo Mesh! Cần ít nhất 3 điểm.");
    //        return;
    //    }

    //    Vector2[] polygonVertices = new Vector2[corners.Length];
    //    for (int i = 0; i < corners.Length; i++)
    //    {
    //        Vector3 localPoint = this.transform.InverseTransformPoint(corners[i]);
    //        polygonVertices[i] = new Vector2(localPoint.x, localPoint.y);
    //    }

    //    GameObject polyExtruderGO = new GameObject("GeneratedMesh", typeof(RectTransform));

    //    PolyExtruder polyExtruder = polyExtruderGO.AddComponent<PolyExtruder>();

    //    float extrusionHeight = 0f;
    //    bool is3D = false;
    //    bool isUsingBottomMesh = true;
    //    bool isUsingColliders = true;

    //    polyExtruder.createPrism(
    //        "GeneratedMesh",
    //        extrusionHeight,
    //        polygonVertices,
    //        Color.grey,
    //        is3D,
    //        isUsingBottomMesh,
    //        isUsingColliders
    //    );

    //    RectTransform rect = polyExtruderGO.GetComponent<RectTransform>();
    //    rect.SetParent(this.transform, false);

    //    Vector3 center = Vector3.zero;
    //    foreach (Vector3 point in corners)
    //    {
    //        center += point;
    //    }
    //    center /= polygonVertices.Length;
    //    rect.pivot = new Vector2(0.5f, 0.5f);
    //    rect.anchoredPosition = center;
    //    rect.localScale = Vector3.one;

    //    Vector3 pos = polyExtruderGO.transform.position;
    //    pos.z = -1;
    //    polyExtruderGO.transform.position = pos;
    //    polyExtruderGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
    //}

    /// <summary>
    /// Hàm CreateBackgroundMesh(Vector3[] corners) có nhiệm vụ tạo mặt nền (floor mesh) từ các điểm đầu vào (thường là đa giác của một phòng), sau đó hiển thị mesh này bằng MeshRenderer và áp dụng vật liệu + collider tương ứng.
    /// </summary>
    private void CreateBackgroundMesh(Vector3[] corners)
    {
        if (backgroundMeshRenderer == null || backgroundMeshFilter == null)
        {
            backgroundMeshRenderer = gameObject.AddComponent<MeshRenderer>();
            backgroundMeshFilter = gameObject.AddComponent<MeshFilter>();
        }

        Mesh mesh = new Mesh();

        int vertexCount = corners.Length;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i] = corners[i];
        }

        // Tạo tam giác theo thứ tự kim đồng hồ
        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        //UV
        Vector2[] uv = new Vector2[vertexCount];

        // Xác định phạm vi X và Y của đa giác
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;

        for (int i = 0; i < vertexCount; i++)
        {
            minX = Mathf.Min(minX, vertices[i].x);
            maxX = Mathf.Max(maxX, vertices[i].x);
            minY = Mathf.Min(minY, vertices[i].y);
            maxY = Mathf.Max(maxY, vertices[i].y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        for (int i = 0; i < vertexCount; i++)
        {
            uv[i] = new Vector2(
                (vertices[i].x - minX) / width,
                (vertices[i].y - minY) / height
            );
        }

        // Cập nhật Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        backgroundMeshFilter.mesh = mesh;

        // Gán vật liệu
        if (item.CompareKindOfItem(kindGroundString))
        {
            backgroundMeshRenderer.material = backgroundMaterialTemp;
        }
        else
        {
            backgroundMeshRenderer.material = itemMaterialTemp;
        }

        AddMeshCollider();

        // Tránh Z-fighting
        backgroundMeshRenderer.material.renderQueue = 3000;
    }

    /// <summary>
    /// Hàm EnableSizePoint(bool statusSizePoint) có nhiệm vụ bật/tắt hiển thị các điểm đo kích thước (SizePoint) trên một đối tượng (thường là sàn nhà hoặc vật thể) dựa theo loại đối tượng và trạng thái yêu cầu.
    /// </summary>
    public void EnableSizePoint(bool statusSizePoint)
    {
        if (item.CompareKindOfItem(kindGroundString))
        {
            for (int i = 0; i < sizePointList.Count; i++)
            {
                sizePointList[i].gameObject.SetActive(statusSizePoint);
                sizePointList[i].AdjustSizePointToCamera();
            }
        }
        else
        {
            for (int i = 1; i < sizePointList.Count; i += 2)
            {
                sizePointList[i].gameObject.SetActive(statusSizePoint);
                sizePointList[i].AdjustSizePointToCamera();
            }
        }
    }

    /// <summary>
    /// Đoạn hàm EnableEdgeText(bool status) có chức năng ẩn hoặc hiện các đối tượng hiển thị thông tin cạnh trong mô hình — bao gồm:
    /// Text độ dài cạnh(edgeLengthTextObjects)
    /// Các đường cạnh(edgeLineRenderers)
    /// Các biểu tượng(icon)
    /// Các đường kéo dài phụ trợ(extensionLineList)
    /// </summary>
    public void EnableEdgeText(bool status)
    {
        for (int i = 0; i < edgeLengthTextObjects.Count; i++)
        {
            edgeLengthTextObjects[i].SetActive(status);
            edgeLineRenderers[i].gameObject.SetActive(status);
        }

        for (int i = 0; i < iconObjects.Count; i++)
        {
            iconObjects[i].SetActive(status);
            extensionLineList[i].SetActive(status);
        }
    }

    /// <summary>
    /// Đoạn code ChangeColor(ColorPicker colorPicker) có chức năng thay đổi texture và màu sắc của vật liệu nền hoặc vật liệu vật thể dựa trên lựa chọn từ ColorPicker (có thể là UI chọn màu hoặc texture).
    /// </summary>
    public void ChangeColor(ColorPicker colorPicker)
    {
        Color newColor = colorPicker.image.sprite.texture.GetPixel(50, 50);
        newColor.a = colorPicker.alpha;

        if (item.CompareKindOfItem(kindGroundString))
        {
            backgroundMaterialTemp.mainTexture = colorPicker.image.sprite.texture;
            backgroundMaterialTemp.SetColor("_Color", Color.white);
            backgroundMaterialTemp.SetVector("_Tiling", new Vector4(1, 1, 0, 0));
            backgroundMeshRenderer.material = backgroundMaterialTemp;
        }
        else
        {
            itemMaterialTemp.mainTexture = colorPicker.image.sprite.texture;
            itemMaterialTemp.SetColor("_Color", newColor);
            itemMaterialTemp.SetVector("_Tiling", new Vector4(1, 1, 0, 0));
            backgroundMeshRenderer.material = itemMaterialTemp;
        }

        item.colorPicker = colorPicker;
        isUsingImageBackground = false;
    }

    /// <summary>
    /// Đoạn code SetDefaultColor() này dùng để thiết lập lại vật liệu nền (backgroundMaterialTemp) và gán vật liệu mới cho một MeshRenderer — thường dùng để reset lại mặt nền (background) về mặc định.
    /// </summary>
    public void SetDefaultColor()
    {
        Material material = new Material(tempMaterial);
        backgroundMaterialTemp.mainTexture = backgroundTexture;
        backgroundMaterialTemp.SetColor("_Color", Color.white);
        backgroundMaterialTemp.SetVector("_Tiling", new Vector4(1, 1, 0, 0));
        backgroundMeshRenderer.material = material;
    }

    /// <summary>
    /// Chức năng của đoạn code này là tìm tất cả GameObject con trực tiếp trong một Transform cha có tên trùng với chuỗi name truyền vào.
    /// </summary>
    private List<GameObject> GetAllGOInParent(string name, Transform parent)
    {
        List<GameObject> edgeIcons = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                edgeIcons.Add(child.gameObject);
            }
        }

        return edgeIcons;
    }
}
