using UnityEngine;
using System.Collections.Generic;

public class ModelView : MonoBehaviour
{
    public Material roomMaterial;
    public BtnController btnController;
    private List<Vector3> basePoints = new List<Vector3>();
    private List<Vector3> heightPoints = new List<Vector3>();
    private GameObject previewWall; // Tường tạm thời

    // Vẽ từng tường với vật liệu tương ứng
    public void CreateWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

        // Gán Layer
        // wall.layer = LayerMask.NameToLayer("PreviewModel");
        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        meshRenderer.material = roomMaterial;

        Mesh mesh = new Mesh();

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
        Vector2[] uv = {
            new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1)
        };
        mesh.uv = uv;


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

    public void DrawPreviewWall(Vector3 base1, Vector3 top1, Vector3 base2, Vector3 top2)
    {
        if (previewWall == null)
        {
            previewWall = CreatePreviewWall(base1, top1, base2, top2);
        }
        else
        {
            UpdatePreviewWall(previewWall, base1, top1, base2, top2);
        }
    }

    private GameObject CreatePreviewWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("PreviewWall");
        wall.transform.SetParent(transform);

        SetLayerRecursively(wall, LayerMask.NameToLayer("PreviewModel"));

        MeshFilter mf = wall.AddComponent<MeshFilter>();
        MeshRenderer mr = wall.AddComponent<MeshRenderer>();
        mr.material = roomMaterial; // Có thể đổi sang vật liệu trong suốt nếu muốn

        Mesh mesh = new Mesh();

        Vector3 forward = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        float thickness = 0.05f;
        Vector3 offset = forward * thickness;

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
            0, 2, 1, 2, 3, 1,
            6, 4, 5, 6, 5, 7,
            4, 0, 1, 4, 1, 5,
            2, 6, 7, 2, 7, 3,
            1, 3, 7, 1, 7, 5,
            4, 6, 2, 4, 2, 0
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;

        return wall;
    }

    private void UpdatePreviewWall(GameObject wall, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        MeshFilter mf = wall.GetComponent<MeshFilter>();
        if (mf == null) return;

        Mesh mesh = new Mesh();
        Vector3 forward = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        float thickness = 0.05f;
        Vector3 offset = forward * thickness;

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
        0, 2, 1, 2, 3, 1,
        6, 4, 5, 6, 5, 7,
        4, 0, 1, 4, 1, 5,
        2, 6, 7, 2, 7, 3,
        1, 3, 7, 1, 7, 5,
        4, 6, 2, 4, 2, 0
    };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }
}
