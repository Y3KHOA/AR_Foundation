using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
    public Material bottomMaterial;

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
            List<Vector2> path2D = room.checkpoints;
            List<float> heights = room.heights;
            List<WallLine> wallLines = room.wallLines;

            if (path2D == null || heights == null || wallLines == null || path2D.Count < 2 || path2D.Count != heights.Count)
                continue;

            List<Vector3> basePts = new();
            List<Vector3> heightPts = new();

            for (int i = 0; i < path2D.Count; i++)
            {
                Vector3 basePos = new(path2D[i].x, 0f, path2D[i].y);
                Vector3 heightPos = new(path2D[i].x, heights[i], path2D[i].y);
                basePts.Add(basePos);
                heightPts.Add(heightPos);
            }

            // Dựng tường từng đoạn
            for (int i = 0; i < basePts.Count - 1; i++)
            {
                // Kiểm tra LineType (nếu có WallLine tương ứng)
                WallLine matchingLine = room.wallLines.Find(line =>
                    IsMatchingSegment(line, basePts[i], basePts[i + 1])
                );

                if (matchingLine != null &&
                    (matchingLine.type == LineType.Door || matchingLine.type == LineType.Window))
                {
                    continue; // Bỏ qua không dựng
                }

                CreateWall(basePts[i], basePts[i + 1], heightPts[i], heightPts[i + 1]);
            }

            // Đoạn cuối khép kín
            if (basePts.Count >= 3)
            {
                WallLine closingLine = room.wallLines.Find(line =>
                    IsMatchingSegment(line, basePts[^1], basePts[0])
                );

                if (closingLine == null || (closingLine.type != LineType.Door && closingLine.type != LineType.Window))
                {
                    CreateWall(basePts[^1], basePts[0], heightPts[^1], heightPts[0]);
                }

                if (basePts[0] != basePts[^1])
                    basePts.Add(basePts[0]);

                CreateFloorMesh(basePts);
            }
        }
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
