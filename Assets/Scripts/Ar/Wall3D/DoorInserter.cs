using UnityEngine;
using System.Collections.Generic;

public class DoorInserter : MonoBehaviour
{
    public Room room;
    public BtnController btnController;

    public Vector2 pa; // vị trí cửa - đầu
    public Vector2 pb; // vị trí cửa - cuối
    public float doorHeight = 0.7f; // 70cm

    private bool doorInserted = false;

    void Update()
    {
        if (btnController.Flag == 1 && !doorInserted)
        {
            InsertDoorToRoom();
            doorInserted = true; // đảm bảo chỉ chèn một lần
        }
    }

    void InsertDoorToRoom()
    {
        // Tìm đoạn wall chứa pa → pb
        for (int i = 0; i < room.checkpoints.Count; i++)
        {
            Vector2 p1 = room.checkpoints[i];
            Vector2 p2 = room.checkpoints[(i + 1) % room.checkpoints.Count];

            if (IsPointOnLineSegment(pa, p1, p2) && IsPointOnLineSegment(pb, p1, p2))
            {
                Debug.Log("Found line segment for door.");

                // Chèn các điểm mới theo thứ tự: p1 → pa → pb → p2
                room.checkpoints.Insert(i + 1, pa);
                room.checkpoints.Insert(i + 2, pb);

                // Tạo WallLines mới (chỉ xử lý tạm đơn giản):
                Vector3 p1_3D = new Vector3(p1.x, 0, p1.y);
                Vector3 pa_3D = new Vector3(pa.x, 0, pa.y);
                Vector3 pb_3D = new Vector3(pb.x, 0, pb.y);
                Vector3 p2_3D = new Vector3(p2.x, 0, p2.y);

                WallLine wall1 = new WallLine(p1_3D, pa_3D, LineType.Wall);
                WallLine door = new WallLine(pa_3D, pb_3D, LineType.Door); // phần cửa
                WallLine wall2 = new WallLine(pb_3D, p2_3D, LineType.Wall);

                room.wallLines.RemoveAt(i); // Xóa đoạn gốc
                room.wallLines.Insert(i, wall2);
                room.wallLines.Insert(i, door);
                room.wallLines.Insert(i, wall1);

                // Nếu cần, tạo điểm cao (70cm) tương ứng:
                Vector3 pa_top = new Vector3(pa.x, doorHeight, pa.y);
                Vector3 pb_top = new Vector3(pb.x, doorHeight, pb.y);

                Debug.Log("Đã thêm cửa từ " + pa + " đến " + pb);
                return;
            }
        }

        Debug.LogWarning("Không tìm thấy đoạn phù hợp để chèn cửa.");
    }

    bool IsPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        float cross = (p.y - a.y) * (b.x - a.x) - (p.x - a.x) * (b.y - a.y);
        if (Mathf.Abs(cross) > 0.01f) return false; // Không cùng đường thẳng

        float dot = (p.x - a.x) * (b.x - a.x) + (p.y - a.y) * (b.y - a.y);
        if (dot < 0) return false;

        float squaredLengthBA = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
        if (dot > squaredLengthBA) return false;

        return true;
    }
}
