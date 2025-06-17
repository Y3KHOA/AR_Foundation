using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;
using System.Linq;
using TMPro;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
    public Material roomMaterial2;
    public Material bottomMaterial;
    public GameObject doorPrefab;
    public GameObject windowPrefab;
    public GameObject modelHolder;

    public GameObject compassPrefab;       // Prefab la bàn 3D

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
            if (room.Compass == Vector2.zero)
            {
                Debug.LogWarning($"[Room {rooms.IndexOf(room)}] Compass chua duoc thiet lap!");
            }
            else
            {
                Debug.Log($"[Room {rooms.IndexOf(room)}] Compass = {room.Compass}, Heading = {room.headingCompass}");
            }

            // === XÓA các đoạn tường ảo (start ≈ end) ===
            room.wallLines.RemoveAll(w =>
            {
                bool isZeroLength = Vector3.Distance(w.start, w.end) < 0.01f;
                if (isZeroLength)
                    Debug.LogWarning($"[REMOVE] Doan {w.type} do dai ≈ 0: {w.start} → {w.end}");
                return isZeroLength;
            });

            // Thiết lập hướng chuẩn dựa trên tường gần mốc ===
            Vector3 compassWorld = new Vector3(room.Compass.x, 0f, room.Compass.y);
            float desiredDirection = room.headingCompass; // Tường gần mốc 
            SetHeadingBasedOnNearestWall(room, compassWorld, desiredDirection);

            Debug.Log("[Heading] Huong thuc dia: " + room.headingCompass);

            // Vẽ sàn trước để không bị các phần khác che khuất
            CreateFloor(room);
            // Vẽ Tọa độ đã check
            CreateCompassObject(room);
            // Tính toán các hướng dựa trên hướng chuẩn.
            // UpdateWallDirections(room);

            // Kiểm tra dữ liệu đầu vào phòng
            if (room.wallLines == null || room.wallLines.Count == 0)
                continue;
            Debug.Log("==== LIST WALLLINES cua phong " + rooms.IndexOf(room) + " ====");
            foreach (var l in room.wallLines)
                Debug.Log($"[LIST] WALLLINES cua phong:{l.type}: {l.start} -> {l.end}");

            // Lấy chiều cao tường cho phòng này
            float roomWallHeight = GetRoomHeight(room);

            // Phân loại các line để xử lý theo thứ tự ưu tiên, loại trừ các đoạn cuối
            List<WallLine> walls = new List<WallLine>();
            List<WallLine> doors = new List<WallLine>();
            List<WallLine> windows = new List<WallLine>();

            // List để lưu trữ các đối tượng đoạn cuối
            List<GameObject> lastSectionObjects = new List<GameObject>();

            // Lần đầu tiên, vẽ TẤT CẢ các đoạn ngoại trừ các đoạn cuối đặc biệt
            foreach (WallLine line in room.wallLines)
            {
                if (line.type == LineType.Wall)
                {
                    // Nếu có một Door/Window nào có cùng start-end (hoặc đảo ngược), thì bỏ qua Wall này
                    bool overlap = room.wallLines.Any(l =>
                        (l.type == LineType.Door || l.type == LineType.Window) &&
                        (
                            (Vector3.Distance(l.start, line.start) < 0.01f && Vector3.Distance(l.end, line.end) < 0.01f) ||
                            (Vector3.Distance(l.start, line.end) < 0.01f && Vector3.Distance(l.end, line.start) < 0.01f)
                        )
                    );
                    if (overlap)
                    {
                        Debug.Log($"[SKIP WALL] Doan nay trung voi cua hoac cua so: {line.start} -> {line.end}");
                        continue;
                    }

                    CreateWallSegment(line, roomWallHeight, room.headingCompass); ;
                }
                else if (line.type == LineType.Door)
                {
                    CreateDoorSegment(line, roomWallHeight);
                }
                else if (line.type == LineType.Window)
                {
                    CreateWindowSegment(line, roomWallHeight);
                }
            }

            Vector2 center = Vector2.zero;
            foreach (var p in room.checkpoints)
                center += p;
            center /= room.checkpoints.Count;
            Debug.Log($"[Room {rooms.IndexOf(room)}] Center of floor: {center}, Compass: {room.Compass}");
        }
    }

    // Hàm vẽ phần tường thông thường
    private GameObject CreateWallSegment(WallLine line, float roomHeight, float headingCompass)
    {
        // Kiểm tra nếu có Door/Window trùng
        bool isOverlapWithDoorOrWindow = RoomStorage.rooms
            .SelectMany(r => r.wallLines)
            .Any(l =>
                (l.type == LineType.Door || l.type == LineType.Window) &&
                (
                    (Vector3.Distance(l.start, line.start) < 0.01f && Vector3.Distance(l.end, line.end) < 0.01f) ||
                    (Vector3.Distance(l.start, line.end) < 0.01f && Vector3.Distance(l.end, line.start) < 0.01f)
                )
            );
        if (isOverlapWithDoorOrWindow)
        {
            Debug.Log($"[SKIP WALL] Trùng cửa/cửa sổ: {line.start} -> {line.end}");
            return CreateDoorSegment(line, roomHeight);
        }

        // Tạo tường
        Vector3 base1 = new Vector3(line.start.x, 0f, line.start.z);
        Vector3 base2 = new Vector3(line.end.x, 0f, line.end.z);
        Vector3 top1 = base1 + Vector3.up * roomHeight;
        Vector3 top2 = base2 + Vector3.up * roomHeight;

        GameObject wallObj = CreateWall(base1, base2, top1, top2);

        // Tính hướng tường
        Vector3 dir = (line.end - line.start).normalized;
        float angleLocal = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float realWorldAngle = (angleLocal + headingCompass + 360f) % 360f;
        string directionLabel = AngleToDirectionLabel(realWorldAngle);

        // Chỉ hiển thị nhãn nếu tường dài hơn 20cm (0.2m)
        float wallLength = Vector3.Distance(line.start, line.end);
        if (wallLength >= 0.2f)
        {
            Vector3 midPoint = (line.start + line.end) / 2f;
            Vector3 direction = line.end - line.start;
            AddDirectionLabel(wallObj, midPoint, directionLabel, direction, realWorldAngle, roomHeight);
        }
        else
        {
            Debug.Log($"[NO LABEL] Wall too short ({wallLength:F2}m): {line.start} → {line.end}");
        }

        return wallObj;
    }

    // Hàm vẽ cửa và phần tường phía trên cửa
    private GameObject CreateDoorSegment(WallLine line, float roomHeight)
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

        GameObject doorObj = CreateWall2(doorBase1, doorBase2, doorTop1, doorTop2); // Sử dụng CreateWall2 cho cửa

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
        return doorObj;
    }

    // Hàm vẽ cửa sổ và phần tường trên/dưới cửa sổ
    private GameObject CreateWindowSegment(WallLine line, float roomHeight)
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

        GameObject windowObj = CreateWall2(windowBase1, windowBase2, windowTop1, windowTop2); // Sử dụng CreateWall2 cho cửa sổ

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

        return windowObj;
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

    // Vẽ từng tường với vật liệu tương ứng
    private GameObject CreateWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

        // Gán Layer
        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        Material instance = new Material(roomMaterial);
        instance.renderQueue = 2000;
        meshRenderer.material = instance;

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

        return wall;
    }

    private GameObject CreateWall2(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

        // Gán Layer
        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        Material instance = new Material(roomMaterial2);
        instance.renderQueue = 3000; // Cửa nên vẽ sau tường
        instance.SetInt("_ZWrite", 1);
        meshRenderer.material = instance;

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

        return wall;
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

    private void CreateCompassObject(Room room)
    {
        if (room.Compass == Vector2.zero)
        {
            Debug.LogWarning($"[Room {RoomStorage.rooms.IndexOf(room)}] Compass chua thiet lap!");
            return;
        }

        Vector3 compassPosition = new Vector3(room.Compass.x, 0.5f, room.Compass.y);
        GameObject compassObject = Instantiate(compassPrefab, compassPosition, Quaternion.Euler(0, room.headingCompass, 0));
        // Gán Layer
        SetLayerRecursively(compassObject, LayerMask.NameToLayer("PreviewModel"));

        if (modelHolder != null)
            compassObject.transform.SetParent(modelHolder.transform, true);
        else
            Debug.LogWarning("Khong tim thay Model3DHolder trong scene!");

        Debug.Log($"[Compass] Room {RoomStorage.rooms.IndexOf(room)} → Position: {compassPosition}, Parent: {(compassObject.transform.parent != null ? compassObject.transform.parent.name : "null")}");
    }

    private void UpdateWallDirections(Room room)
    {
        foreach (WallLine line in room.wallLines)
        {
            Vector3 dir = (line.end - line.start).normalized;

            // Góc so với trục Z (Bắc)
            float angleToNorth = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // Cộng với góc chuẩn la bàn
            float realWorldAngle = (angleToNorth + room.headingCompass + 360f) % 360f;

            // (tuỳ chọn) gán vào line nếu bạn muốn lưu
            // line.directionAngle = realWorldAngle;

            // Gợi ý hướng chữ
            string directionLabel = AngleToDirectionLabel(realWorldAngle);

            Debug.Log($"[Heading][WallDir] {line.start} → {line.end} = {realWorldAngle:0.0}° ({directionLabel})");
        }
    }

    private string AngleToDirectionLabel(float degree)
    {
        if (degree < 0) degree += 360;

        if ((degree >= 0 && degree < 7.5f) || degree >= 352.5f) return "Bắc";
        if (degree < 22.5f) return "Bắc";
        if (degree < 37.5f) return "Đông Bắc";
        if (degree < 52.5f) return "Đông Bắc";
        if (degree < 67.5f) return "Đông Bắc";
        if (degree < 82.5f) return "Đông";
        if (degree < 97.5f) return "Đông";
        if (degree < 112.5f) return "Đông";
        if (degree < 127.5f) return "Đông Nam";
        if (degree < 142.5f) return "Đông Nam";
        if (degree < 157.5f) return "Đông Nam";
        if (degree < 172.5f) return "Nam";
        if (degree < 187.5f) return "Nam";
        if (degree < 202.5f) return "Nam";
        if (degree < 217.5f) return "Tây Nam";
        if (degree < 232.5f) return "Tây Nam";
        if (degree < 247.5f) return "Tây Nam";
        if (degree < 262.5f) return "Tây";
        if (degree < 277.5f) return "Tây";
        if (degree < 292.5f) return "Tây";
        if (degree < 307.5f) return "Tây Bắc";
        if (degree < 322.5f) return "Tây Bắc";
        if (degree < 337.5f) return "Tây Bắc";
        return "Bắc";
    }

    // void AddDirectionLabel(GameObject wallObject, Vector3 midPoint, string label)
    void AddDirectionLabel(GameObject wallObject, Vector3 midPoint, string label, Vector3 directionVector, float realWorldAngle, float roomHeight)
    {
        GameObject textObj = new GameObject("DirLabel");
        textObj.transform.SetParent(wallObject.transform);
        textObj.transform.position = midPoint + Vector3.up * (roomHeight + 0.3f); // cao hơn tường một chút
        // Gán Layer
        SetLayerRecursively(textObj, LayerMask.NameToLayer("PreviewModel"));

        // Thay vì TextMesh, dùng TextMeshPro
        TextMeshPro text = textObj.AddComponent<TextMeshPro>();
        text.text = $"{realWorldAngle:0.0}°: {label}";
        text.fontSize = 1.5f;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;

        // Tuỳ chọn: Thêm outline và/hoặc bóng để dễ nhìn
        text.fontStyle = FontStyles.Bold;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;

        // Hướng chữ chạy song song với tường
        Vector3 right = directionVector.normalized; // chạy dọc theo tường
        if (right.sqrMagnitude < 0.001f) right = Vector3.right;

        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.Cross(up, right); // mặt chữ hướng ra ngoài

        // Kiểm tra nếu forward đang hướng vào trong (camera phía trước)
        // if (Vector3.Dot(forward, Camera.main.transform.position - midPoint) < 0)
        // {
        //     forward = forward; // đảo lại để quay ra ngoài
        // }

        // Gán rotation
        textObj.transform.rotation = Quaternion.LookRotation(forward, up);
    }

    private WallLine FindNearestWall(Vector3 referencePoint, List<WallLine> walls)
    {
        const float PRIORITY_RADIUS = 0.3f;

        WallLine closestWall = null;
        float minDist = float.MaxValue;

        foreach (var wall in walls)
        {
            // Tính khoảng cách chính xác nhất từ điểm đến đoạn tường
            float dist = DistancePointToLineSegment(referencePoint, wall.start, wall.end);

            if (dist <= PRIORITY_RADIUS)
            {
                Debug.Log($"[Heading] tim thay tuong gan Compass: {wall.start} to {wall.end}, dist = {dist:0.00}");
                return wall;
            }

            if (dist < minDist)
            {
                minDist = dist;
                closestWall = wall;
            }
        }

        Debug.Log($"[Heading] Khong co tuong nao gan Compass. tuong gan nhat: {closestWall?.start} to {closestWall?.end}, dist = {minDist:0.00}");
        return closestWall;
    }

    // Hàm hỗ trợ tính khoảng cách từ điểm tới đoạn thẳng
    private float DistancePointToLineSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 lineDir = end - start;
        float lineLengthSquared = lineDir.sqrMagnitude;

        if (lineLengthSquared == 0.0f)
            return Vector3.Distance(point, start); // Đoạn thẳng chiều dài = 0

        float t = Vector3.Dot(point - start, lineDir) / lineLengthSquared;
        t = Mathf.Clamp01(t);

        Vector3 projection = start + t * lineDir;
        return Vector3.Distance(point, projection);
    }

    private void SetHeadingBasedOnNearestWall(Room room, Vector3 referencePoint, float desiredDirection)
    {
        WallLine refWall = FindNearestWall(referencePoint, room.wallLines);

        if (refWall == null)
        {
            Debug.LogWarning("[Heading] Khong tim thay tuong chuan!");
            return;
        }

        Vector3 dirCandidate = (refWall.end - refWall.start).normalized;
        float angleToNorthCandidate = Mathf.Atan2(dirCandidate.x, dirCandidate.z) * Mathf.Rad2Deg;
        float headingCandidate = (desiredDirection - angleToNorthCandidate + 360f) % 360f;

        // Kiểm tra chiều ngược lại để tránh bị sai chiều
        Vector3 dirReverse = (refWall.start - refWall.end).normalized;
        float angleToNorthReverse = Mathf.Atan2(dirReverse.x, dirReverse.z) * Mathf.Rad2Deg;
        float headingReverse = (desiredDirection - angleToNorthReverse + 360f) % 360f;

        // Chọn góc nào gần góc gốc ban đầu nhất (để đảm bảo chiều đúng)
        float diffCandidate = Mathf.Abs(Mathf.DeltaAngle(room.headingCompass, headingCandidate));
        float diffReverse = Mathf.Abs(Mathf.DeltaAngle(room.headingCompass, headingReverse));

        float finalHeading = diffCandidate <= diffReverse ? headingCandidate : headingReverse;

        room.headingCompass = finalHeading;

        Debug.Log($"[Heading] Tuong chuan: {refWall.start} to {refWall.end}");
        Debug.Log($"[Heading] Huong mong muon: {desiredDirection}, final headingCompass = {finalHeading:0.0}");
    }

}