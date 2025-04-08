using UnityEngine;
using System.Collections.Generic;

public class MiniMapCameraController : MonoBehaviour
{
    public Camera miniMapCamera;
    public BtnController btnController;
    public LineRenderer lineRenderer; // LineRenderer để vẽ đường
    public Vector3 offset = new Vector3(0, 7, -7); // Vị trí MiniMapCamera
    public float padding = 1.5f; // Độ nới để không bị cắt hình
    public float tiltAngle = 45f; // Góc nghiêng để nhìn thấy chiều cao

    void LateUpdate()
    {
        UpdateCameraView();
    }

    void UpdateCameraView()
    {
        if (btnController == null || miniMapCamera == null)
            return;

        List<Vector3> basePoints = btnController.GetBasePoints();
        List<Vector3> heightPoints = btnController.GetHeightPoints();

        if (basePoints.Count == 0) return;

        // Tính toán bounding box (giới hạn không gian của các điểm)
        Bounds bounds = CalculateBounds(basePoints, heightPoints);

        // Điều chỉnh vị trí camera để không bị mất đối tượng
        Vector3 center = bounds.center;
        Vector3 cameraPosition = center + new Vector3(0, bounds.extents.y + offset.y, -bounds.extents.z - offset.z);
        miniMapCamera.transform.position = cameraPosition;

        // Xoay Camera MiniMap nhìn vào trung tâm bounding box
        miniMapCamera.transform.LookAt(center);

        // Điều chỉnh khoảng nhìn để không bị cắt
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.z) * padding;

        // Nếu dùng camera Ortho (dạng bản đồ 2D)
        if (miniMapCamera.orthographic)
        {
            miniMapCamera.orthographicSize = maxSize / 2;
        }
        else
        {
            miniMapCamera.fieldOfView = Mathf.Clamp(maxSize * 2, 30f, 90f); // Đảm bảo góc nhìn hợp lý
        }

        // Vẽ đường bằng LineRenderer
        DrawPaths(basePoints, heightPoints);
    }

    // Tính toán Bounds
    Bounds CalculateBounds(List<Vector3> basePoints, List<Vector3> heightPoints)
    {
        Bounds bounds = new Bounds(basePoints[0], Vector3.zero);
        foreach (Vector3 point in basePoints)
            bounds.Encapsulate(point);
        foreach (Vector3 point in heightPoints)
            bounds.Encapsulate(point);
        return bounds;
    }

    // Vẽ các đoạn đường giữa các điểm
    void DrawPaths(List<Vector3> basePoints, List<Vector3> heightPoints)
    {
        // Kiểm tra nếu có đủ điểm để vẽ
        if (basePoints.Count < 2) return;

        // Thiết lập số lượng điểm trong LineRenderer
        lineRenderer.positionCount = basePoints.Count;

        // Chuyển đổi tất cả các điểm 3D thành tọa độ 2D trên MiniMap
        for (int i = 0; i < basePoints.Count; i++)
        {
            Vector3 worldPosition = basePoints[i];
            Vector3 miniMapPosition = WorldToMiniMap(worldPosition);
            lineRenderer.SetPosition(i, miniMapPosition);
        }

        // Nếu có các đường nối giữa các điểm
        for (int i = 0; i < basePoints.Count - 1; i++)
        {
            Vector3 startPos = basePoints[i];
            Vector3 endPos = basePoints[i + 1];
            DrawLine(startPos, endPos);
        }
    }

    // Chuyển đổi từ vị trí thế giới (3D) thành vị trí 2D trên MiniMap
    Vector3 WorldToMiniMap(Vector3 worldPosition)
    {
        // Áp dụng tỉ lệ và loại bỏ chiều cao (Y) để chỉ giữ lại vị trí 2D
        float miniMapX = worldPosition.x;
        float miniMapZ = worldPosition.z;

        // Có thể thêm các điều chỉnh như tỉ lệ giữa không gian 3D và MiniMap nếu cần
        return new Vector3(miniMapX, miniMapZ, 0f);
    }

    // Vẽ các đoạn đường giữa các checkpoint
    void DrawLine(Vector3 start, Vector3 end)
    {
        Vector3 startPos = WorldToMiniMap(start);
        Vector3 endPos = WorldToMiniMap(end);

        // Tạo một GameObject mới với LineRenderer nếu cần
        GameObject lineObject = new GameObject("Line");
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        // Tùy chỉnh các thuộc tính của LineRenderer như màu sắc, độ rộng
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default")); // Sử dụng shader mặc định cho line
        line.startColor = Color.red;
        line.endColor = Color.red;
    }
}
