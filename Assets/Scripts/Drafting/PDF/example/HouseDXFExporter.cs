// using UnityEngine;
// using netDxf;
// using netDxf.Entities;
// using netDxf.Tables;
// using System.IO;

// public class HouseDXFExporter : MonoBehaviour
// {
//     [ContextMenu("Export House to DXF")]
//     public static void ExportHouse()
//     {
//         DxfDocument dxf = new DxfDocument();

//         // Kích thước nhà và thành phần (mét)
//         float width = 5f; // Chiều rộng
//         float height = 20f; // Chiều cao
//         float wallThickness = 0.2f; // Độ dày tường
//         float doorWidth = 1.0f; // Chiều rộng cửa
//         float windowWidth = 0.8f; // Chiều rộng cửa sổ
//         float windowGap = 0.05f; // Khoảng cách giữa cửa sổ và tường

//         // Tọa độ căn giữa (A4 paper center)
//         float centerX = 105f / 2f; // A4 width (mm) / 2
//         float centerY = 297f / 2f; // A4 height (mm) / 2
//         float ox = centerX - width / 2f; // Tọa độ x của góc dưới trái nhà
//         float oy = centerY - height / 2f; // Tọa độ y của góc dưới trái nhà

//         // Tường ngoài (hình chữ nhật)
//         UnityEngine.Vector2[] outer = {
//             new UnityEngine.Vector2(ox, oy),
//             new UnityEngine.Vector2(ox + width, oy),
//             new UnityEngine.Vector2(ox + width, oy + height),
//             new UnityEngine.Vector2(ox, oy + height)
//         };

//         // Vẽ tường ngoài bằng Polyline (độ dày rõ ràng)
//         Polyline wallOuter = new Polyline();
//         foreach (var pt in outer)
//         {
//             wallOuter.Vertexes.Add(new PolylineVertex(new netDxf.Vector3(pt.x, pt.y, 0)));
//         }
//         wallOuter.IsClosed = true;
//         dxf.Entities.Add(wallOuter);

//         // Vẽ tường trong (offset để tạo độ dày)
//         Polyline wallInner = new Polyline();
//         foreach (var pt in OffsetPolygon(outer, -wallThickness))
//         {
//             wallInner.Vertexes.Add(new PolylineVertex(new netDxf.Vector3(pt.x, pt.y, 0)));
//         }
//         wallInner.IsClosed = true;
//         dxf.Entities.Add(wallInner);

//         // Cửa (ở mặt ngang dưới)
//         float doorCenterX = ox + width / 2f;
//         float doorY = oy;
//         UnityEngine.Vector2 doorL = new UnityEngine.Vector2(doorCenterX - doorWidth / 2f, doorY);
//         UnityEngine.Vector2 doorR = new UnityEngine.Vector2(doorCenterX + doorWidth / 2f, doorY);
//         dxf.Entities.Add(new Line(ToDxfV3(doorL), ToDxfV3(doorR)));

//         // Cửa sổ ở 3 mặt còn lại (trái, phải, trên)
//         float winOffset = windowGap;
//         // Trái
//         UnityEngine.Vector2 winL1 = new UnityEngine.Vector2(ox - winOffset, oy + height / 2 - windowWidth / 2);
//         UnityEngine.Vector2 winL2 = new UnityEngine.Vector2(ox - winOffset, oy + height / 2 + windowWidth / 2);
//         dxf.Entities.Add(new Line(ToDxfV3(winL1), ToDxfV3(winL2)));

//         // Phải
//         UnityEngine.Vector2 winR1 = new UnityEngine.Vector2(ox + width + winOffset, oy + height / 2 - windowWidth / 2);
//         UnityEngine.Vector2 winR2 = new UnityEngine.Vector2(ox + width + winOffset, oy + height / 2 + windowWidth / 2);
//         dxf.Entities.Add(new Line(ToDxfV3(winR1), ToDxfV3(winR2)));

//         // Trên
//         UnityEngine.Vector2 winT1 = new UnityEngine.Vector2(ox + width / 2 - windowWidth / 2, oy + height + winOffset);
//         UnityEngine.Vector2 winT2 = new UnityEngine.Vector2(ox + width / 2 + windowWidth / 2, oy + height + winOffset);
//         dxf.Entities.Add(new Line(ToDxfV3(winT1), ToDxfV3(winT2)));

//         // Lưu file DXF
//         string filePath = Path.Combine(Application.dataPath, "HouseModel.dxf");
//         dxf.Save(filePath);
//         Debug.Log($"✅ DXF exported to: {filePath}");
//     }

//     static netDxf.Vector3 ToDxfV3(UnityEngine.Vector2 v) => new netDxf.Vector3(v.x, v.y, 0);

//     static UnityEngine.Vector2[] OffsetPolygon(UnityEngine.Vector2[] pts, float offset)
//     {
//         return new UnityEngine.Vector2[] {
//             new UnityEngine.Vector2(pts[0].x + offset, pts[0].y + offset),
//             new UnityEngine.Vector2(pts[1].x - offset, pts[1].y + offset),
//             new UnityEngine.Vector2(pts[2].x - offset, pts[2].y - offset),
//             new UnityEngine.Vector2(pts[3].x + offset, pts[3].y - offset)
//         };
//     }
// }
