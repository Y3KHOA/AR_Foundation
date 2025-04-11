using UnityEngine;
using System.Collections.Generic;

public class RoomModelBuilder : MonoBehaviour
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

        // Tạo 2 mặt: trước và sau
        Vector3[] vertices = {
        p1, p2, p3, p4,  // Mặt trước
        p3, p4, p1, p2   // Mặt sau (đảo ngược thứ tự)
    };

        int[] triangles = {
        0, 2, 1, 2, 3, 1,  // Mặt trước
        4, 6, 5, 6, 7, 5   // Mặt sau
    };

        float width = Vector3.Distance(p1, p3);
        float height = Vector3.Distance(p1, p2);

        Vector2[] uv = {
        new Vector2(0, 0), new Vector2(0, height), new Vector2(width, 0), new Vector2(width, height), // UV mặt trước
        new Vector2(0, 0), new Vector2(0, height), new Vector2(width, 0), new Vector2(width, height)  // UV mặt sau
    };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv; // Thêm UV để hiển thị texture đúng cách
        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); // Đảm bảo không bị lỗi hiển thị

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }
}
