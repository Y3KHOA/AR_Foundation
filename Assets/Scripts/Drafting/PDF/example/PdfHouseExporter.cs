using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PdfHouseExporter : MonoBehaviour
{
    static float houseWidth = 5000f;   // 5m
    static float houseHeight = 20000f; // 20m
    static float wallThickness = 200f;
    [ContextMenu("Export House PDF")]
    public static void ExportHousePDF()
    {
        List<Vector2> outer = new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(houseWidth, 0),
            new Vector2(houseWidth, houseHeight),
            new Vector2(0, houseHeight),
            new Vector2(0, 0)
        };

        // string path = Path.Combine(Application.dataPath, "HousePlan.pdf");
        string path = Path.Combine("/storage/emulated/0/Download", "XHeroScan/PDF/Drawing_Tester_House.pdf");
        ExportPDF(outer, path, wallThickness);
        Debug.Log($"PDF exported to: {path}");
    }

    static void ExportPDF(List<Vector2> polygon, string path, float wallThickness)
    {
        Document document = new Document(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
        document.Open();

        PdfContentByte cb = writer.DirectContent;
        cb.SetLineWidth(0.5f);
        cb.SetRGBColorStroke(0, 0, 0);

        // Calculate bounding box
        Vector2 min = polygon[0];
        Vector2 max = polygon[0];
        foreach (var pt in polygon)
        {
            min = Vector2.Min(min, pt);
            max = Vector2.Max(max, pt);
        }

        Vector2 size = max - min;
        float maxW = 500f;
        float maxH = 700f;
        float scale = Mathf.Min(maxW / size.x, maxH / size.y);
        float offsetX = (PageSize.A4.Width - size.x * scale) / 2f;
        float offsetY = (PageSize.A4.Height - size.y * scale) / 2f;
        Vector2 shift = -min;

        // Draw walls (polygon)
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
        }

        // Draw door on bottom side
        float doorWidth = 1000f;
        Vector2 doorCenter = new Vector2(houseWidth / 2f, 0);
        DrawSymbol(cb, doorCenter, 0, doorWidth, scale, shift, offsetX, offsetY, "door");

        // Draw windows on 3 other sides (centered)
        DrawSymbol(cb, new Vector2(houseWidth, houseHeight / 2), 90, 800f, scale, shift, offsetX, offsetY, "window");
        DrawSymbol(cb, new Vector2(houseWidth / 2, houseHeight), 180, 800f, scale, shift, offsetX, offsetY, "window");
        DrawSymbol(cb, new Vector2(0, houseHeight / 2), -90, 800f, scale, shift, offsetX, offsetY, "window");

        document.Close();
    }

    static void DrawSymbol(PdfContentByte cb, Vector2 center, float angleDeg, float width, float scale, Vector2 shift, float offsetX, float offsetY, string type)
    {
        float half = width / 2f;
        Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        Vector2 p1 = center + dir * -half;
        Vector2 p2 = center + dir * half;

        Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);
        Vector2 pp1 = Convert(p1);
        Vector2 pp2 = Convert(p2);

        if (type == "door")
        {
            cb.SetLineWidth(1f);
            cb.SetRGBColorStroke(0, 0, 255); // blue for door
        }
        else if (type == "window")
        {
            cb.SetLineWidth(0.5f);
            cb.SetRGBColorStroke(255, 0, 0); // red for window
        }

        cb.MoveTo(pp1.x, pp1.y);
        cb.LineTo(pp2.x, pp2.y);
        cb.Stroke();
        cb.SetRGBColorStroke(0, 0, 0); // reset to black
    }
}
