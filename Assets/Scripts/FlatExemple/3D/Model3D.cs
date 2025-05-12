using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
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
            List<Vector2> checkpoints = room.checkpoints;
            List<float> heights = room.heights;
            List<WallLine> wallLines = room.wallLines;

            if (checkpoints == null || heights == null || wallLines == null ||
                checkpoints.Count < 2 || checkpoints.Count != heights.Count)
                continue;

            // Dựng tường theo từng đoạn
            for (int i = 0; i < checkpoints.Count - 1; i++)
            {
                Vector3 base1 = new Vector3(checkpoints[i].x, 0f, checkpoints[i].y);
                Vector3 base2 = new Vector3(checkpoints[i + 1].x, 0f, checkpoints[i + 1].y);
                Vector3 top1 = base1 + Vector3.up * heights[i];
                Vector3 top2 = base2 + Vector3.up * heights[i + 1];

                WallLine match = wallLines.Find(w => IsSameSegment2D(w.start, w.end, base1, base2));

                if (match != null && (match.type == LineType.Door || match.type == LineType.Window))
                {
                    // Xử lý cửa hoặc cửa sổ
                    Vector3 doorStart = match.start;
                    Vector3 doorEnd = match.end;

                    // Bảo đảm hướng
                    if (Vector3.Distance(doorStart, base2) < Vector3.Distance(doorStart, base1))
                        (doorStart, doorEnd) = (doorEnd, doorStart);

                    float wallHeight = Mathf.Max(heights[i], heights[i + 1]);
                    float doorHeight = Mathf.Min(heights[i], heights[i + 1]); // giả định chiều cao cửa nằm ở checkpoint

                    Vector3 bTop = doorStart + Vector3.up * doorHeight;
                    Vector3 cTop = doorEnd + Vector3.up * doorHeight;

                    Vector3 bUpper = doorStart + Vector3.up * wallHeight;
                    Vector3 cUpper = doorEnd + Vector3.up * wallHeight;

                    // Vẽ trái cửa
                    if (Vector3.Distance(base1, doorStart) > 0.01f)
                        CreateWall(base1, doorStart, doorStart + Vector3.up * wallHeight, top1);

                    // Vẽ cửa
                    // CreateWall2(doorStart, doorEnd, cTop, bTop);
                    CreateWall2(doorStart, doorEnd, bTop, cTop);

                    // Vẽ tường trên cửa
                    if (wallHeight > doorHeight + 0.01f)
                        CreateWall(bTop, cTop, bUpper, cUpper);

                    // Vẽ phải cửa
                    if (Vector3.Distance(doorEnd, base2) > 0.01f)
                        CreateWall(doorEnd, base2, top2, doorEnd + Vector3.up * wallHeight);
                }
                else
                {
                    // Tường thường
                    CreateWall(base1, base2, top1, top2);
                }
            }

            // Tạo sàn
            List<Vector3> basePts = new();
            foreach (var pt in checkpoints)
                basePts.Add(new Vector3(pt.x, 0f, pt.y));

            if (basePts.Count >= 3)
            {
                if (basePts[0] != basePts[^1])
                    basePts.Add(basePts[0]);

                CreateFloorMesh(basePts);
            }
        }
    }

    private bool IsSameSegment2D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, float tolerance = 0.01f)
    {
        Vector2 A1 = new Vector2(a1.x, a1.z);
        Vector2 A2 = new Vector2(a2.x, a2.z);
        Vector2 B1 = new Vector2(b1.x, b1.z);
        Vector2 B2 = new Vector2(b2.x, b2.z);

        return (Vector2.Distance(A1, B1) < tolerance && Vector2.Distance(A2, B2) < tolerance)
            || (Vector2.Distance(A1, B2) < tolerance && Vector2.Distance(A2, B1) < tolerance);
    }

    private bool IsMatchingSegment(WallLine line, Vector3 a, Vector3 b)
    {
        Vector2 a2D = new(a.x, a.z);
        Vector2 b2D = new(b.x, b.z);
        Vector2 l1 = new(line.start.x, line.start.z);
        Vector2 l2 = new(line.end.x, line.end.z);

        return (Approximately(a2D, l1) && Approximately(b2D, l2)) ||
               (Approximately(a2D, l2) && Approximately(b2D, l1));
    }

    private bool Approximately(Vector2 v1, Vector2 v2, float tolerance = 0.01f)
    {
        return Vector2.Distance(v1, v2) <= tolerance;
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
        meshRenderer.material = bottomMaterial;

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
}
