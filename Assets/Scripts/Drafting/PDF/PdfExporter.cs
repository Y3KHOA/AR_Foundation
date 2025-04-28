using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PdfExporter
{
    static float wallThickness = 200f;

    static HashSet<float> usedTextY = new HashSet<float>();
    static HashSet<float> usedTextX = new HashSet<float>();
    public static void ExportMultiplePolygonsToPDF(List<List<Vector2>> allPolygons, string path, float wallThickness)
    {
        if (allPolygons == null || allPolygons.Count == 0) return;

        Document document = new Document(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
        document.Open();

        PdfContentByte cb = writer.DirectContent;
        BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.SetFontAndSize(baseFont, 10);
        // cb.SetLineWidth(0.5f);
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

            for (int i = 0; i < polygon.Count - 1; i++)
            {
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[i + 1];

                Vector2 d = (p2 - p1).normalized;
                Vector2 perp = new Vector2(-d.y, d.x);
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
                cb.ClosePathStroke();

                // DrawDimensionLine(cb, pa, pb, -300f, scale, shift, offsetX, offsetY, $"{pa} mm");
                // DrawDimensionLine(cb, pb, pc, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pb, pc):0} mm");
                // DrawDimensionLine(cb, pc, pd, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pc, pd):0} mm");
                // DrawDimensionLine(cb, pd, new Vector2(0,0), -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pd, pa):0} mm");
            }

            // Đóng polygon (vẽ cạnh cuối nối đầu và cuối)
            Vector2 last = polygon[^1];
            Vector2 first = polygon[0];

            Vector2 d1 = (first - last).normalized;
            Vector2 perp1 = new Vector2(-d1.y, d1.x);
            Vector2 offset1 = perp1 * wallThickness * 0.5f;

            Vector2 a1 = last + offset1;
            Vector2 b1 = first + offset1;
            Vector2 c1 = first - offset1;
            Vector2 d21 = last - offset1;

            Vector2 Convert1(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);

            Vector2 pa1 = Convert1(a1);
            Vector2 pb1 = Convert1(b1);
            Vector2 pc1 = Convert1(c1);
            Vector2 pd1 = Convert1(d21);

            cb.MoveTo(pa1.x, pa1.y);
            cb.LineTo(pb1.x, pb1.y);
            cb.LineTo(pc1.x, pc1.y);
            cb.LineTo(pd1.x, pd1.y);
            cb.ClosePathStroke();

            // DrawDimensionLine(cb, pa1, pb1, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pa1, pb1):0} mm");
            // DrawDimensionLine(cb, pb1, pc1, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pb1, pc1):0} mm");
            // DrawDimensionLine(cb, pc1, pd1, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pc1, pd1):0} mm");
            // DrawDimensionLine(cb, pd1, pa1, -300f, scale, shift, offsetX, offsetY, $"{Vector2.Distance(pd1, pa1):0} mm");
        }
        document.Close();
    }

    static void DrawDimensionLine(PdfContentByte cb, Vector2 p1, Vector2 p2, float offset, float scale, Vector2 shift, float offsetX, float offsetY, string label)
    {
        Vector2 dir = (p2 - p1).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);

        Vector2 sp1 = Convert(p1 + perp * offset);
        Vector2 sp2 = Convert(p2 + perp * offset);
        Vector2 ep1 = Convert(p1);
        Vector2 ep2 = Convert(p2);

        // Vẽ đường kích thước + mũi tên
        cb.SetLineWidth(2f);
        cb.MoveTo(sp1.x, sp1.y);
        cb.LineTo(ep1.x, ep1.y);
        cb.MoveTo(sp2.x, sp2.y);
        cb.LineTo(ep2.x, ep2.y);
        cb.MoveTo(sp1.x, sp1.y);
        cb.LineTo(sp2.x, sp2.y);
        cb.Stroke();

        // Mũi tên
        float arrowSize = 5f;
        Vector2 arrowDir = (sp2 - sp1).normalized;
        Vector2 arrowNormal = new Vector2(-arrowDir.y, arrowDir.x);

        void DrawArrow(Vector2 pos, Vector2 dirVec)
        {
            Vector2 left = pos - dirVec * arrowSize + arrowNormal * arrowSize;
            Vector2 right = pos - dirVec * arrowSize - arrowNormal * arrowSize;
            cb.MoveTo(left.x, left.y);
            cb.LineTo(pos.x, pos.y);
            cb.LineTo(right.x, right.y);
            cb.Stroke();
        }

        DrawArrow(sp1, arrowDir);
        DrawArrow(sp2, -arrowDir);

        // Text
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.BeginText();
        cb.SetFontAndSize(bf, 15);

        Vector2 mid = (sp1 + sp2) / 2;
        Vector2 textDir = (sp1 - ep1).normalized;

        // Đảm bảo căn chỉnh chữ ra ngoài
        float offsetDistance = 200f * scale;
        Vector2 textPos = mid + textDir * offsetDistance;

        // Kiểm tra trùng vị trí chữ
        bool isHorizontal = Mathf.Abs(dir.y) < 0.5f;
        float spacingStep = 30f * scale;
        int tries = 0;

        while (true)
        {
            if (isHorizontal)
            {
                if (!usedTextY.Contains(textPos.y))
                {
                    usedTextY.Add(textPos.y);
                    break;
                }
                textPos += textDir * spacingStep;
            }
            else
            {
                if (!usedTextX.Contains(textPos.x))
                {
                    usedTextX.Add(textPos.x);
                    break;
                }
                textPos += textDir * spacingStep;
            }

            if (++tries > 10) break; // tránh vòng lặp vô hạn
        }

        // Vẽ chữ ở vị trí căn chỉnh
        cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, label, textPos.x, textPos.y, 0);
        cb.EndText();
    }
}