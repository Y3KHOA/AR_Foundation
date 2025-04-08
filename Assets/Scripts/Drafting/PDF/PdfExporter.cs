using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PdfExporter
{
    public static void ExportPolygonToPDF(List<Vector2> points2D, List<float> distances, string filePath, string unit = "m")
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var pt in points2D)
        {
            if (pt.x < minX) minX = pt.x;
            if (pt.y < minY) minY = pt.y;
            if (pt.x > maxX) maxX = pt.x;
            if (pt.y > maxY) maxY = pt.y;
        }

        float drawingWidth = maxX - minX;
        float drawingHeight = maxY - minY;

        float pageWidth = 595f;
        float pageHeight = 842f;
        float scale = Mathf.Min((pageWidth - 100) / drawingWidth, (pageHeight - 100) / drawingHeight);
        float offsetX = (pageWidth - drawingWidth * scale) / 2f;
        float offsetY = (pageHeight - drawingHeight * scale) / 2f;

        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            Document doc = new Document(new Rectangle(pageWidth, pageHeight));
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            cb.SetLineWidth(1f);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            cb.SetFontAndSize(bf, 10);

            for (int i = 0; i < points2D.Count; i++)
            {
                Vector2 p1 = points2D[i];
                Vector2 p2 = points2D[(i + 1) % points2D.Count];

                float x1 = offsetX + (p1.x - minX) * scale;
                float y1 = offsetY + (p1.y - minY) * scale;
                float x2 = offsetX + (p2.x - minX) * scale;
                float y2 = offsetY + (p2.y - minY) * scale;

                cb.MoveTo(x1, y1);
                cb.LineTo(x2, y2);

                if (i < distances.Count)
                {
                    string distanceText = $"{distances[i]:F2} {unit}";
                    float midX = (x1 + x2) / 2f;
                    float midY = (y1 + y2) / 2f;
                    cb.BeginText();
                    cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, distanceText, midX, midY + 5f, 0);
                    cb.EndText();
                }
            }

            cb.Stroke();
            doc.Close();
        }

        Debug.Log("PDF bản vẽ đã được tạo tại: " + filePath);
    }
}
