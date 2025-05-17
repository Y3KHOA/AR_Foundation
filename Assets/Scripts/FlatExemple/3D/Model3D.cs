using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;
using System.Linq;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
    public Material roomMaterial2;
    public Material bottomMaterial;
    public GameObject doorPrefab;
    public GameObject windowPrefab;

    void Start()
    {
        List<Room> rooms = RoomStorage.rooms;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("Không có Room nào trong RoomStorage.");
            return;
        }

        foreach (Room room in rooms)
        {
            // Vẽ sàn trước để không bị các phần khác che khuất
            CreateFloor(room);

            // Kiểm tra dữ liệu đầu vào phòng
            if (room.wallLines == null || room.wallLines.Count == 0)
                continue;
            Debug.Log("==== LIST WALLLINES cua phong " + rooms.IndexOf(room) + " ====");
            foreach (var l in room.wallLines)
                Debug.Log($"LIST WALLLINES cua phong:{l.type}: {l.start} -> {l.end}");
            SnapWallLinePoints(room.wallLines);

            // Lấy chiều cao tường cho phòng này
            float roomWallHeight = GetRoomHeight(room);

            // Debug.Log($"Phòng có chiều cao tường: {roomWallHeight}");

            // Phân loại các line để xử lý theo thứ tự ưu tiên
            List<WallLine> walls = new List<WallLine>();
            List<WallLine> doors = new List<WallLine>();
            List<WallLine> windows = new List<WallLine>();

            foreach (WallLine line in room.wallLines)
            {
                switch (line.type)
                {
                    case LineType.Wall: walls.Add(line); break;
                    case LineType.Door: doors.Add(line); break;
                    case LineType.Window: windows.Add(line); break;
                }
            }

            // Vẽ tường trước
            foreach (WallLine wall in walls)
            {
                CreateWallSegment(wall, roomWallHeight);
            }

            // Vẽ cửa (và phần tường phía trên cửa nếu có)
            foreach (WallLine door in doors)
            {
                CreateDoorSegment(door, roomWallHeight);
            }

            // Vẽ cửa sổ (và phần tường trên/dưới cửa sổ)
            foreach (WallLine window in windows)
            {
                CreateWindowSegment(window, roomWallHeight);
            }
        }
    }


    // Lấy chiều cao của phòng từ room.heights
    private float GetRoomHeight(Room room)
    {
        // Nếu có chiều cao trong danh sách, lấy giá trị đầu tiên
        if (room.heights != null && room.heights.Count > 0)
        {
            return Mathf.Max(0.01f, room.heights[0]);
        }

        // Giá trị mặc định nếu không có thông tin chiều cao
        return 3.0f; // Giả định chiều cao tường mặc định là 3 mét
    }

    // Hàm tạo sàn từ các điểm checkpoint
    private void CreateFloor(Room room)
    {
        if (room.checkpoints != null && room.checkpoints.Count >= 3)
        {
            List<Vector3> floorPoints = new List<Vector3>();

            foreach (var point in room.checkpoints)
            {
                // Chuyển đổi từ điểm 2D sang điểm 3D tại mặt sàn (y=0)
                floorPoints.Add(new Vector3(point.x, 0f, point.y));
            }

            // Đảm bảo đóng polygon bằng cách thêm điểm đầu tiên vào cuối nếu cần
            if (floorPoints[0] != floorPoints[floorPoints.Count - 1])
            {
                floorPoints.Add(floorPoints[0]);
            }

            CreateFloorMesh(floorPoints);
        }
    }


    // Hàm vẽ phần tường thông thường
    private void CreateWallSegment(WallLine line, float roomHeight)
    {
        Vector3 start = line.start;
        Vector3 end = line.end;

        float baseY = 0f; // Tường luôn bắt đầu từ mặt sàn
        float height = roomHeight; // Sử dụng chiều cao từ room.heights

        Vector3 base1 = new Vector3(start.x, baseY, start.z);
        Vector3 base2 = new Vector3(end.x, baseY, end.z);
        Vector3 top1 = base1 + Vector3.up * height;
        Vector3 top2 = base2 + Vector3.up * height;

        CreateWall(base1, base2, top1, top2);
    }

    // Hàm vẽ cửa và phần tường phía trên cửa (nếu có)
    private void CreateDoorSegment(WallLine line, float roomHeight)
    {
        Vector3 start = line.start;
        Vector3 end = line.end;

        // Thông số cửa
        float doorHeight = Mathf.Max(0.01f, line.Height); // Chiều cao cửa từ line.Height
        float totalWallHeight = roomHeight; // Tổng chiều cao tường từ room.heights

        // 1. Phần cửa - từ sàn đến độ cao cửa
        Vector3 doorBase1 = new Vector3(start.x, 0f, start.z);
        Vector3 doorBase2 = new Vector3(end.x, 0f, end.z);
        Vector3 doorTop1 = doorBase1 + Vector3.up * doorHeight;
        Vector3 doorTop2 = doorBase2 + Vector3.up * doorHeight;

        CreateWall2(doorBase1, doorBase2, doorTop1, doorTop2); // Sử dụng CreateWall2 cho cửa

        // 2. Phần tường phía trên cửa (nếu có)
        float wallHeightAboveDoor = totalWallHeight - doorHeight;

        if (wallHeightAboveDoor > 0.01f)
        {
            Vector3 aboveBase1 = doorTop1;
            Vector3 aboveBase2 = doorTop2;
            Vector3 aboveTop1 = aboveBase1 + Vector3.up * wallHeightAboveDoor;
            Vector3 aboveTop2 = aboveBase2 + Vector3.up * wallHeightAboveDoor;

            CreateWall(aboveBase1, aboveBase2, aboveTop1, aboveTop2);
        }
    }

    // Hàm vẽ cửa sổ và phần tường trên/dưới cửa sổ
    private void CreateWindowSegment(WallLine line, float roomHeight)
    {
        Vector3 start = line.start;
        Vector3 end = line.end;

        // Thông số cửa sổ
        float totalWallHeight = roomHeight; // Tổng chiều cao tường từ room.heights
        float windowDistance = Mathf.Max(0f, line.distanceHeight); // Khoảng cách từ sàn đến đáy cửa sổ
        float windowHeight = Mathf.Max(0.01f, line.Height); // Chiều cao cửa sổ

        // 1. Phần tường dưới cửa sổ
        if (windowDistance > 0.01f)
        {
            Vector3 lowerBase1 = new Vector3(start.x, 0f, start.z);
            Vector3 lowerBase2 = new Vector3(end.x, 0f, end.z);
            Vector3 lowerTop1 = lowerBase1 + Vector3.up * windowDistance;
            Vector3 lowerTop2 = lowerBase2 + Vector3.up * windowDistance;

            CreateWall(lowerBase1, lowerBase2, lowerTop1, lowerTop2);
        }

        // 2. Phần cửa sổ
        Vector3 windowBase1 = new Vector3(start.x, windowDistance, start.z);
        Vector3 windowBase2 = new Vector3(end.x, windowDistance, end.z);
        Vector3 windowTop1 = windowBase1 + Vector3.up * windowHeight;
        Vector3 windowTop2 = windowBase2 + Vector3.up * windowHeight;

        CreateWall2(windowBase1, windowBase2, windowTop1, windowTop2); // Sử dụng CreateWall2 cho cửa sổ

        // 3. Phần tường trên cửa sổ
        float aboveHeight = totalWallHeight - windowDistance - windowHeight;

        if (aboveHeight > 0.01f)
        {
            Vector3 upperBase1 = windowTop1;
            Vector3 upperBase2 = windowTop2;
            Vector3 upperTop1 = upperBase1 + Vector3.up * aboveHeight;
            Vector3 upperTop2 = upperBase2 + Vector3.up * aboveHeight;

            CreateWall(upperBase1, upperBase2, upperTop1, upperTop2);
        }
    }

    // Vẽ từng tường với vật liệu tương ứng
    private void CreateWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

        // Gán Layer
        // wall.layer = LayerMask.NameToLayer("PreviewModel");
        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        meshRenderer.material = roomMaterial;

        UnityEngine.Mesh mesh = new UnityEngine.Mesh();

        // Hướng vuông góc với mặt tường để tạo độ dày
        Vector3 forward = Vector3.Cross(p2 - p1, p4 - p1).normalized;
        float thickness = 0.05f;
        Vector3 offset = forward * thickness;

        // Tám đỉnh của khối hộp (bức tường có độ dày)
        Vector3[] vertices = new Vector3[8];
        vertices[0] = p1;
        vertices[1] = p2;
        vertices[2] = p3;
        vertices[3] = p4;

        vertices[4] = p1 + offset;
        vertices[5] = p2 + offset;
        vertices[6] = p3 + offset;
        vertices[7] = p4 + offset;

        int[] triangles = {
            // Mặt trước
            0, 2, 1, 2, 3, 1,
            // Mặt sau
            6, 4, 5, 6, 5, 7,
            // Trái
            4, 0, 1, 4, 1, 5,
            // Phải
            2, 6, 7, 2, 7, 3,
            // Trên
            1, 3, 7, 1, 7, 5,
            // Dưới
            4, 6, 2, 4, 2, 0
        };

        // Vector2[] uv = new Vector2[8]; // có thể chỉnh UV chi tiết nếu cần, nhưng giữ đơn giản
        Vector2[] uv = new Vector2[8];
        for (int i = 0; i < 8; i++)
            uv[i] = new Vector2(vertices[i].x, vertices[i].y);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }
    private void CreateWall2(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

        // Gán Layer
        // wall.layer = LayerMask.NameToLayer("PreviewModel");
        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        meshRenderer.material = roomMaterial2;

        UnityEngine.Mesh mesh = new UnityEngine.Mesh();

        // Hướng vuông góc với mặt tường để tạo độ dày
        Vector3 forward = Vector3.Cross(p2 - p1, p4 - p1).normalized;
        float thickness = 0.05f;
        Vector3 offset = forward * thickness;

        // Tám đỉnh của khối hộp (bức tường có độ dày)
        Vector3[] vertices = new Vector3[8];
        vertices[0] = p1;
        vertices[1] = p2;
        vertices[2] = p3;
        vertices[3] = p4;

        vertices[4] = p1 + offset;
        vertices[5] = p2 + offset;
        vertices[6] = p3 + offset;
        vertices[7] = p4 + offset;

        int[] triangles = {
            // Mặt trước
            0, 2, 1, 2, 3, 1,
            // Mặt sau
            6, 4, 5, 6, 5, 7,
            // Trái
            4, 0, 1, 4, 1, 5,
            // Phải
            2, 6, 7, 2, 7, 3,
            // Trên
            1, 3, 7, 1, 7, 5,
            // Dưới
            4, 6, 2, 4, 2, 0
        };

        // Vector2[] uv = new Vector2[8]; // có thể chỉnh UV chi tiết nếu cần, nhưng giữ đơn giản
        Vector2[] uv = new Vector2[8];
        for (int i = 0; i < 8; i++)
            uv[i] = new Vector2(vertices[i].x, vertices[i].y);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void CreateFloorMesh(List<Vector3> basePts)
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(transform);
        SetLayerRecursively(floor, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter mf = floor.AddComponent<MeshFilter>();
        MeshRenderer mr = floor.AddComponent<MeshRenderer>();
        mr.material = bottomMaterial;

        // Triangulate bằng LibTessDotNet
        Tess tess = new Tess();

        // Chuyển sang contour 2D (XZ plane)
        ContourVertex[] contour = new ContourVertex[basePts.Count];
        for (int i = 0; i < basePts.Count; i++)
        {
            contour[i].Position = new Vec3(basePts[i].x, basePts[i].z, 0); // XZ plane
            contour[i].Data = null;
        }

        tess.AddContour(contour);
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

        // Chuyển vertices ngược lại về Vector3 (trên mặt đất, y = 0)
        Vector3[] vertices = new Vector3[tess.Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(tess.Vertices[i].Position.X, 0, tess.Vertices[i].Position.Y);
        }

        int[] triangles = tess.Elements;

        // Tạo thêm mặt sau
        Vector3[] allVertices = new Vector3[vertices.Length * 2];
        int[] allTriangles = new int[triangles.Length * 2 + (vertices.Length * 6)];

        // Sao chép vertices cho mặt sau
        for (int i = 0; i < vertices.Length; i++)
        {
            allVertices[i] = vertices[i];
            allVertices[i + vertices.Length] = vertices[i] + Vector3.up * 0.1f;  // Tạo độ dày cho mặt sau
        }

        // Sao chép triangles cho mặt trước
        for (int i = 0; i < triangles.Length; i++)
        {
            allTriangles[i] = triangles[i];
        }

        // Tạo triangles cho mặt sau, đảo thứ tự của các chỉ số vertices
        int offset = vertices.Length; // offset là nơi bắt đầu chỉ số của mặt sau
        // Mặt sau (đảo ngược thứ tự để flip mặt)
        for (int i = 0; i < triangles.Length; i += 3)
        {
            allTriangles[i + triangles.Length + 0] = triangles[i + 0] + offset;
            allTriangles[i + triangles.Length + 1] = triangles[i + 2] + offset;
            allTriangles[i + triangles.Length + 2] = triangles[i + 1] + offset;
        }

        // Nối tam giác các cạnh bên vào danh sách tam giác tổng hợp
        List<int> allTrianglesList = new List<int>(); // Đặt lại danh sách tam giác tổng hợp
        allTrianglesList.AddRange(allTriangles);  // Thêm tam giác mặt trước và mặt sau vào

        // Tạo Mesh
        UnityEngine.Mesh mesh = new UnityEngine.Mesh();
        mesh.vertices = allVertices;
        mesh.triangles = allTrianglesList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Đảm bảo mesh có thể hiển thị mặt sau
        mesh.SetTriangles(allTrianglesList.ToArray(), 0);

        // Tạo material cho mesh, và đảm bảo không có culling
        mr.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off); // Tắt culling để hiển thị cả mặt trước và sau

        mf.mesh = mesh;
    }

    void SnapWallLinePoints(List<WallLine> wallLines, float epsilon = 0.001f)
    {
        for (int i = 0; i < wallLines.Count; i++)
        {
            for (int j = 0; j < wallLines.Count; j++)
            {
                if (i == j) continue;

                // Snap điểm cuối của line i với điểm đầu của line j nếu gần nhau
                if (Vector3.Distance(wallLines[i].end, wallLines[j].start) < epsilon)
                    wallLines[j].start = wallLines[i].end;
                // Snap điểm đầu của line i với điểm cuối của line j nếu gần nhau
                if (Vector3.Distance(wallLines[i].start, wallLines[j].end) < epsilon)
                    wallLines[j].end = wallLines[i].start;
            }
        }
    }

}
