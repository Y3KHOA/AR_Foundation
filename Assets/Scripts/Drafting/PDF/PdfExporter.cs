using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PdfExporter
{
    public static void ExportMultiplePolygonsToPDF(List<List<Vector2>> allPolygons, List<List<float>> allDistances, string path, string unit)
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

                float x1 = (p1.x + shift.x) * scale + offsetX;
                float y1 = (p1.y + shift.y) * scale + offsetY;
                float x2 = (p2.x + shift.x) * scale + offsetX;
                float y2 = (p2.y + shift.y) * scale + offsetY;

                cb.MoveTo(x1, y1);
                cb.LineTo(x2, y2);
                cb.Stroke();

                // Label chiều dài cạnh
                if (allDistances != null && polygonIndex < allDistances.Count && i < allDistances[polygonIndex].Count)
                {
                    float midX = (x1 + x2) / 2;
                    float midY = (y1 + y2) / 2;
                    string label = $"{allDistances[polygonIndex][i]:0.00} {unit}";
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, label, midX, midY + 5f, 0);
                    cb.EndText();
                }
            }

            // Đóng đoạn cuối (khép kín)
            Vector2 last = polygon[^1];
            Vector2 first = polygon[0];
            float x11 = (last.x + shift.x) * scale + offsetX;
            float y11 = (last.y + shift.y) * scale + offsetY;
            float x22 = (first.x + shift.x) * scale + offsetX;
            float y22 = (first.y + shift.y) * scale + offsetY;

            cb.MoveTo(x11, y11);
            cb.LineTo(x22, y22);
            cb.Stroke();
            // Hiển thị label chiều dài cạnh cuối
            if (allDistances != null && polygonIndex < allDistances.Count && allDistances[polygonIndex].Count > 0)
            {
                float midX = (x11 + x22) / 2;
                float midY = (y11 + y22) / 2;
                string label = $"{allDistances[polygonIndex][^1]:0.00} {unit}";
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_CENTER, label, midX, midY + 5f, 0);
                cb.EndText();
            }
        }

        document.Close();
    }
}