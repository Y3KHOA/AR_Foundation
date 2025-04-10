using UnityEngine;
using System.Collections.Generic;

public class Model3D : MonoBehaviour
{
    public Material roomMaterial;
    private List<Vector3> basePoints = new List<Vector3>();
    private List<Vector3> heightPoints = new List<Vector3>();

    // Nhận dữ liệu đo từ BtnController
    public void SetRoomData(List<Vector3> basePts, List<Vector3> heightPts)
    {
        basePoints = basePts;
        heightPoints = heightPts;
    }

    public void BuildWalls()
    {
        int count = basePoints.Count;
        if (count < 2) return;

        // Chỉ vẽ tường giữa điểm mới nhất và điểm trước đó
        Vector3 base1 = basePoints[count - 2];
        Vector3 top1 = heightPoints[count - 2];
        Vector3 base2 = basePoints[count - 1];
        Vector3 top2 = heightPoints[count - 1];

        CreateWall(base1, top1, base2, top2);
    }
    // Xóa tường cũ trước khi vẽ lại
    private void ClearExistingWalls()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // DestroyImmediate(transform.GetChild(i).gameObject);
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    // Vẽ từng tường với vật liệu tương ứng
    private void CreateWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(transform);

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

        Vector2[] uv = new Vector2[8]; // có thể chỉnh UV chi tiết nếu cần, nhưng giữ đơn giản

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

}
