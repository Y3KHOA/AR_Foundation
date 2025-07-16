using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static Mesh CreateRoomMesh(List<Vector2> points)
    {
        Debug.Log($"[MeshGenerator] Start CreateRoomMesh: points={points.Count}");

        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i] = new Vector3(points[i].x, 0, points[i].y);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;

        int[] triangles = Triangulate(points);
        List<int> doubleSidedTriangles = new List<int>(triangles);

        // Thêm mặt đảo ngược để tạo 2 mặt
        for (int i = 0; i < triangles.Length; i += 3)
        {
            doubleSidedTriangles.Add(triangles[i]);
            doubleSidedTriangles.Add(triangles[i + 2]);
            doubleSidedTriangles.Add(triangles[i + 1]);
        }

        mesh.triangles = doubleSidedTriangles.ToArray();

        return mesh;
    }

    // === Ear Clipping đơn giản ===
    private static int[] Triangulate(List<Vector2> polyPoints)
    {
        List<int> indices = new List<int>();
        int n = polyPoints.Count;
        if (n < 3)
            return indices.ToArray();

        List<int> V = new List<int>();

        // LUÔN LUÔN KIỂM TRA AREA() ĐỂ ĐẢM BẢO HƯỚNG ĐÚNG
        if (Area(polyPoints) > 0)
        {
            for (int v = 0; v < n; v++)
                V.Add(v);
        }
        else
        {
            for (int v = 0; v < n; v++)
                V.Add((n - 1) - v);
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                break;

            int u = v; if (nv <= u) u = 0;
            v = u + 1; if (nv <= v) v = 0;
            int w = v + 1; if (nv <= w) w = 0;

            if (Snip(polyPoints, u, v, w, nv, V))
            {
                int a = V[u], b = V[v], c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                V.RemoveAt(v);
                nv--;
                count = 2 * nv;
            }
        }

        return indices.ToArray();
    }

    private static float Area(List<Vector2> polyPoints)
    {
        int n = polyPoints.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = polyPoints[p];
            Vector2 qval = polyPoints[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A * 0.5f;
    }

    private static bool Snip(List<Vector2> polyPoints, int u, int v, int w, int n, List<int> V)
    {
        Vector2 A = polyPoints[V[u]];
        Vector2 B = polyPoints[V[v]];
        Vector2 C = polyPoints[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;

        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w)) continue;
            Vector2 P = polyPoints[V[p]];
            if (InsideTriangle(A, B, C, P)) return false;
        }
        return true;
    }

    private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax = C.x - B.x, ay = C.y - B.y;
        float bx = A.x - C.x, by = A.y - C.y;
        float cx = B.x - A.x, cy = B.y - A.y;
        float apx = P.x - A.x, apy = P.y - A.y;
        float bpx = P.x - B.x, bpy = P.y - B.y;
        float cpx = P.x - C.x, cpy = P.y - C.y;

        float aCROSSbp = ax * bpy - ay * bpx;
        float cCROSSap = cx * apy - cy * apx;
        float bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
    public static float CalculateArea(List<Vector2> polyPoints)
    {
        int n = polyPoints.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = polyPoints[p];
            Vector2 qval = polyPoints[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A * 0.5f;
    }
}
