using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
    public Material bottomMaterial;

    void Start()
    {
        List<List<Vector2>> allPoints = DataTransfer.Instance.GetAllPoints();     // Danh sách các mạch 2D
        List<List<float>> allHeights = DataTransfer.Instance.GetAllHeights();     // Chiều cao tương ứng từng điểm trong từng mạch

        if (allPoints == null || allHeights == null || allPoints.Count == 0 || allHeights.Count == 0)
        {
            Debug.LogWarning("Không có dữ liệu 3D để dựng mô hình.");
            return;
        }

        for (int pathIndex = 0; pathIndex < allPoints.Count; pathIndex++)
        {
            List<Vector2> path2D = allPoints[pathIndex];
            List<float> heights = allHeights[pathIndex];

            // Bỏ qua nếu dữ liệu không khớp
            if (path2D == null || heights == null || path2D.Count < 2 || path2D.Count != heights.Count)
                continue;

            List<Vector3> basePts = new List<Vector3>();
            List<Vector3> heightPts = new List<Vector3>();

            for (int i = 0; i < path2D.Count; i++)
            {
                Vector3 basePos = new Vector3(path2D[i].x, 0f, path2D[i].y);
                Vector3 heightPos = new Vector3(path2D[i].x, heights[i], path2D[i].y);
                basePts.Add(basePos);
                heightPts.Add(heightPos);
            }

            // Vẽ từng cạnh giữa 2 điểm liên tiếp
            for (int i = 0; i < basePts.Count - 1; i++)
            {
                // CreateWall(basePts[i], heightPts[i], basePts[i + 1], heightPts[i + 1]);
                CreateWall(basePts[i], basePts[i + 1], heightPts[i], heightPts[i + 1]);
            }

            // Khép kín mạch nếu có ít nhất 3 điểm
            if (basePts.Count >= 3)
            {
                CreateWall(basePts[^1], heightPts[^1], basePts[0], heightPts[0]);

                // Đảm bảo polygon khép kín cho vẽ sàn
                if (basePts[0] != basePts[^1])
                    basePts.Add(basePts[0]);

                CreateFloorMesh(basePts);
            }
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
        Vector3 forward = Vector3.Cross(p2 - p1, p3 - p1).normalized;
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
