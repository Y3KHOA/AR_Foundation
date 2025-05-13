using UnityEngine;
using System.Collections.Generic;
using LibTessDotNet;
using System.Linq;

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
            if (room.wallLines == null || room.wallLines.Count == 0)
                continue;

            foreach (WallLine line in room.wallLines)
            {
                Vector3 start = line.start;
                Vector3 end = line.end;
                float baseY = line.distanceHeight;
                float height = line.Height;

                Vector3 baseStart = new Vector3(start.x, baseY, start.z);
                Vector3 baseEnd = new Vector3(end.x, baseY, end.z);
                Vector3 topStart = baseStart + Vector3.up * height;
                Vector3 topEnd = baseEnd + Vector3.up * height;

                switch (line.type)
                {
                    case LineType.Wall:
                        {
                            // Tìm các đoạn cắt (door/window) cùng nằm trên đoạn này
                            List<WallLine> cutouts = room.wallLines
                                .Where(w => (w.type == LineType.Door || w.type == LineType.Window) &&
                                            IsSameSegment2D(w.start, w.end, line.start, line.end))
                                .OrderBy(w => Vector3.Distance(line.start, w.start))
                                .ToList();

                            Vector3 currentStart = line.start;

                            foreach (WallLine cut in cutouts)
                            {
                                Vector3 cutStart = cut.start;
                                Vector3 cutEnd = cut.end;

                                if (Vector3.Distance(currentStart, cutStart) > 0.01f)
                                {
                                    CreateWall(
                                        new Vector3(currentStart.x, baseY, currentStart.z),
                                        new Vector3(cutStart.x, baseY, cutStart.z),
                                        new Vector3(currentStart.x, baseY + height, currentStart.z),
                                        new Vector3(cutStart.x, baseY + height, cutStart.z)
                                    );
                                }

                                currentStart = cutEnd;
                            }

                            if (Vector3.Distance(currentStart, line.end) > 0.01f)
                            {
                                CreateWall(
                                    new Vector3(currentStart.x, baseY, currentStart.z),
                                    new Vector3(line.end.x, baseY, line.end.z),
                                    new Vector3(currentStart.x, baseY + height, currentStart.z),
                                    new Vector3(line.end.x, baseY + height, line.end.z)
                                );
                            }

                            break;
                        }

                    case LineType.Door:
                        {
                            // Cửa sát sàn
                            Vector3 top1 = baseStart + Vector3.up * height;
                            Vector3 top2 = baseEnd + Vector3.up * height;
                            CreateWall2(baseStart, baseEnd, top1, top2);
                            break;
                        }

                    case LineType.Window:
                        {
                            // Cửa sổ giữa tường → chia thành 3 phần: dưới - cửa - trên
                            Vector3 midTop1 = baseStart + Vector3.up * height;
                            Vector3 midTop2 = baseEnd + Vector3.up * height;
                            Vector3 fullTop1 = start + Vector3.up * (baseY + height);
                            Vector3 fullTop2 = end + Vector3.up * (baseY + height);

                            // Phần dưới cửa sổ
                            if (baseY > 0.01f)
                                CreateWall(start, end, baseStart, baseEnd);

                            // Phần cửa sổ
                            CreateWall2(baseStart, baseEnd, midTop1, midTop2);

                            // Phần trên cửa sổ
                            if (height < 2.5f) // cửa sổ không cao tới trần
                                CreateWall(midTop1, midTop2, fullTop1, fullTop2);
                            break;
                        }
                }
            }

                // Dựng sàn
                if (room.checkpoints != null && room.checkpoints.Count >= 3)
                {
                    List<Vector3> basePts = new();
                    foreach (var pt in room.checkpoints)
                        basePts.Add(new Vector3(pt.x, 0f, pt.y));

                    if (basePts[0] != basePts[^1])
                        basePts.Add(basePts[0]);

                    CreateFloorMesh(basePts);
                }
        }
    }
    bool IsSameSegment2D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, float threshold = 0.01f)
    {
        // Chuyển sang 2D (bỏ Y)
        Vector2 a1_2D = new Vector2(a1.x, a1.z);
        Vector2 a2_2D = new Vector2(a2.x, a2.z);
        Vector2 b1_2D = new Vector2(b1.x, b1.z);
        Vector2 b2_2D = new Vector2(b2.x, b2.z);

        // So sánh không phân biệt thứ tự điểm (thuận hoặc ngược chiều)
        return
            (Vector2.Distance(a1_2D, b1_2D) < threshold && Vector2.Distance(a2_2D, b2_2D) < threshold) ||
            (Vector2.Distance(a1_2D, b2_2D) < threshold && Vector2.Distance(a2_2D, b1_2D) < threshold);
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
