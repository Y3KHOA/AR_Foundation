using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class PdfExporter
{
    public static byte[] GeneratePdfAsBytes(List<Room> rooms, float wallThickness)
{
    if (rooms == null || rooms.Count == 0) return null;

    using (MemoryStream memoryStream = new MemoryStream())
    {
        Document document = new Document(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
        document.Open();

        PdfContentByte cb = writer.DirectContent;
        BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.SetFontAndSize(baseFont, 10);
        cb.SetLineWidth(2f);
        cb.SetRGBColorStroke(0, 0, 0);

        // === Tính global bounding box ===
        Vector2 globalMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 globalMax = new Vector2(float.MinValue, float.MinValue);

        foreach (var room in rooms)
        {
            foreach (var pt in room.checkpoints)
            {
                globalMin = Vector2.Min(globalMin, pt);
                globalMax = Vector2.Max(globalMax, pt);
            }
        }

        Vector2 globalSize = globalMax - globalMin;
        float maxWidth = 500f, maxHeight = 700f;
        float scale = Mathf.Min(maxWidth / globalSize.x, maxHeight / globalSize.y);
        float offsetX = (PageSize.A4.Width - globalSize.x * scale) / 2f;
        float offsetY = (PageSize.A4.Height - globalSize.y * scale) / 2f;
        Vector2 shift = -globalMin;

        // === Convert helper ===
        Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);

        // === Vẽ từng Room ===
        foreach (var room in rooms)
        {
            var polygon = room.checkpoints;
            if (polygon.Count < 2) continue;

            // Nếu điểm đầu trùng điểm cuối thì loại bỏ
            if (Vector2.Distance(polygon[0], polygon[^1]) < 0.01f)
            {
                polygon = polygon.Take(polygon.Count - 1).ToList();
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[(i + 1) % polygon.Count];

                Vector2 dir = (p2 - p1).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x);
                Vector2 offset = perp * wallThickness * 0.5f;

                Vector2 pa = p1 + offset;
                Vector2 pb = p2 + offset;
                Vector2 pc = p2 - offset;
                Vector2 pd = p1 - offset;

                Vector2 cpa = Convert(pa);
                Vector2 cpb = Convert(pb);
                Vector2 cpc = Convert(pc);
                Vector2 cpd = Convert(pd);

                // Vẽ hình chữ nhật tường
                cb.MoveTo(cpa.x, cpa.y);
                cb.LineTo(cpb.x, cpb.y);
                cb.LineTo(cpc.x, cpc.y);
                cb.LineTo(cpd.x, cpd.y);
                cb.ClosePath();
                cb.Stroke();

                // Đo chiều dài tường
                Vector2 cp1 = Convert(p1);
                Vector2 cp2 = Convert(p2);
                DrawDimensionLine(cb, cp1, cp2, -30f, $"{Vector2.Distance(p1, p2):0.00}");

                // Đo chiều dày tường (vuông góc)
                DrawDimensionLine(cb, cpa, cpd, 20f, $"{wallThickness:0.0}");
            }

            // Vẽ cửa và cửa sổ
            foreach (var wall in room.wallLines)
            {
                if (wall.type != LineType.Door && wall.type != LineType.Window) continue;

                Vector2 startConverted = Convert(wall.start);
                Vector2 endConverted = Convert(wall.end);

                DrawSymbol(cb, startConverted, endConverted, wall.type.ToString().ToLower());
            }
        }

        document.Close();
        return memoryStream.ToArray();
    }
}



    //hàm vẽ cửa và cửa sổ
    static void DrawSymbol(PdfContentByte cb, Vector2 p1, Vector2 p2, string type)
    {
        Vector2 center = (p1 + p2) * 0.5f;
        if (type == "door")
        {
            cb.SetLineWidth(1f);
            cb.SetRGBColorStroke(0, 0, 255); // Blue

            // Dùng p1 là bản lề, p2 là đầu cánh cửa
            cb.MoveTo(p1.x, p1.y);
            cb.LineTo(p2.x, p2.y);
            cb.Stroke();

            // Vẽ cung từ p2 quay về 90 độ từ p1
            float radius = Vector2.Distance(p1, p2);
            float angleStart = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;

            cb.Arc(p1.x - radius, p1.y - radius, p1.x + radius, p1.y + radius,
                   -angleStart, -90);
            cb.Stroke();
        }

        else if (type == "window")
        {
            cb.SetLineWidth(0.5f);
            cb.SetRGBColorStroke(255, 0, 0); // Red

            Vector2 dir = (p2 - p1).normalized;
            Vector2 norm = new Vector2(-dir.y, dir.x) * 50f;

            Vector2 winA1 = p1 + norm;
            Vector2 winA2 = p1 - norm;
            Vector2 winB1 = p2 + norm;
            Vector2 winB2 = p2 - norm;

            cb.MoveTo(winA1.x, winA1.y);
            cb.LineTo(winA2.x, winA2.y);
            cb.MoveTo(winB1.x, winB1.y);
            cb.LineTo(winB2.x, winB2.y);
            cb.Stroke();
        }

        cb.SetRGBColorStroke(0, 0, 0); // Reset màu
    }

    static void DrawDimensionLine(PdfContentByte cb, Vector2 p1, Vector2 p2, float offsetDistance, string label)
    {
        // Tính toán hướng chính và vuông góc
        Vector2 dir = (p2 - p1).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x); // vuông góc 90 độ

        // Offset điểm ra ngoài theo hướng vuông góc
        Vector2 p1Offset = p1 + perp * offsetDistance;
        Vector2 p2Offset = p2 + perp * offsetDistance;

        // Vẽ đường kích thước
        cb.SetLineWidth(0.5f);
        cb.MoveTo(p1Offset.x, p1Offset.y);
        cb.LineTo(p2Offset.x, p2Offset.y);
        cb.Stroke();

        // Vẽ đường gióng từ điểm thực ra điểm offset
        cb.MoveTo(p1.x, p1.y);
        cb.LineTo(p1Offset.x, p1Offset.y);
        cb.MoveTo(p2.x, p2.y);
        cb.LineTo(p2Offset.x, p2Offset.y);
        cb.Stroke();

        // Vẽ mũi tên 2 đầu
        float arrowSize = 3f;

        void DrawArrow(Vector2 pos, Vector2 direction)
        {
            Vector2 dir2=-direction.normalized;

            Vector2 left = pos - dir2 * arrowSize + new Vector2(-dir2.y, dir2.x) * (arrowSize * 0.5f);
            Vector2 right = pos - dir2 * arrowSize - new Vector2(-dir2.y, dir2.x) * (arrowSize * 0.5f);

            cb.MoveTo(left.x, left.y);
            cb.LineTo(pos.x, pos.y);
            cb.LineTo(right.x, right.y);
            cb.Stroke();
        }

        DrawArrow(p1Offset, dir);
        DrawArrow(p2Offset, -dir);

        // Vẽ text
        Vector2 midPoint = (p1Offset + p2Offset) / 2f;

        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.BeginText();
        cb.SetFontAndSize(bf, 7);

        // Tính góc xoay text
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Để text đứng thẳng dễ đọc, chỉnh lại góc nếu cần
        if (angle > 90f || angle < -90f)
        {
            angle += 180f;
        }

        cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, label, midPoint.x, midPoint.y, angle);
        cb.EndText();
    }
}