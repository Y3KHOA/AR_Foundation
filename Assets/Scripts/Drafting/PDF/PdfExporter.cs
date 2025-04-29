using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PdfExporter
{
    
    public static void ExportMultiplePolygonsToPDF(List<List<Vector2>> allPolygons, string path, float wallThickness)
    {
        if (allPolygons == null || allPolygons.Count == 0) return;

        Document document = new Document(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
        document.Open();

        PdfContentByte cb = writer.DirectContent;
        BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.SetFontAndSize(baseFont, 10);
        cb.SetLineWidth(2f);
        cb.SetRGBColorStroke(0, 0, 0);

        // === Tính bounding box toàn bộ ===
        Vector2 globalMin = allPolygons[0][0];
        Vector2 globalMax = allPolygons[0][0];
        foreach (var polygon in allPolygons)
        {
            foreach (var pt in polygon)
            {
                globalMin = Vector2.Min(globalMin, pt);
                globalMax = Vector2.Max(globalMax, pt);
            }
        }
        Vector2 globalSize = globalMax - globalMin;

        // === Scale & canh giữa trang A4 ===
        float maxWidth = 500f;
        float maxHeight = 700f;
        float scale = Mathf.Min(maxWidth / globalSize.x, maxHeight / globalSize.y);
        float offsetX = (PageSize.A4.Width - globalSize.x * scale) / 2f;
        float offsetY = (PageSize.A4.Height - globalSize.y * scale) / 2f;
        Vector2 shift = -globalMin;

        // === Vẽ từng polygon ===
        for (int polygonIndex = 0; polygonIndex < allPolygons.Count; polygonIndex++)
        {
            var polygon = new List<Vector2>(allPolygons[polygonIndex]); // tạo bản sao để xử lý
            if (polygon.Count < 2) continue;

            // Loại bỏ điểm trùng đầu-cuối nếu có
            if (Vector2.Distance(polygon[0], polygon[^1]) < 0.01f)
            {
                polygon.RemoveAt(polygon.Count - 1);
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[(i + 1) % polygon.Count];

                // Tính vector unit hướng p1 -> p2
                Vector2 d = (p2 - p1).normalized;

                // Vector vuông góc với đường thẳng (trái tay)
                Vector2 perp = new Vector2(-d.y, d.x);

                // 2 vector offset sang trái/phải
                Vector2 offset = perp * wallThickness * 0.5f;

                Vector2 a = p1 + offset;
                Vector2 b = p2 + offset;
                Vector2 c = p2 - offset;
                Vector2 d2 = p1 - offset;

                Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);

                Vector2 pa = Convert(a);
                Vector2 pb = Convert(b);
                Vector2 pc = Convert(c);
                Vector2 pd = Convert(d2);

                cb.MoveTo(pa.x, pa.y);
                cb.LineTo(pb.x, pb.y);
                cb.LineTo(pc.x, pc.y);
                cb.LineTo(pd.x, pd.y);
                cb.ClosePath(); // tự động nối kín 4 đỉnh
                cb.Stroke();

                Vector2 p1Converted = Convert(p1);
                Vector2 p2Converted = Convert(p2);
                DrawDimensionLine(cb, p1Converted, p2Converted, -30f, $"{Vector2.Distance(p1, p2):0.00} m");

            }
        }
        document.Close();
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
        float arrowSize = 10f;

        void DrawArrow(Vector2 pos, Vector2 direction)
        {
            Vector2 left = pos - direction * arrowSize + new Vector2(-direction.y, direction.x) * (arrowSize * 0.5f);
            Vector2 right = pos - direction * arrowSize - new Vector2(-direction.y, direction.x) * (arrowSize * 0.5f);

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
        cb.SetFontAndSize(bf, 14);

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