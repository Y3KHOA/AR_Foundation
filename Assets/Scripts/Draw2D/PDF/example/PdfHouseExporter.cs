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

    static HashSet<float> usedTextY = new HashSet<float>();
    static HashSet<float> usedTextX = new HashSet<float>();

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

        string path;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // Trên PC
        string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/Downloads";
        path = Path.Combine(downloadsPath, "XHeroScan/PDF/Example_House.pdf");
#else
    // Trên Android
    path = Path.Combine("/storage/emulated/0/Download", "XHeroScan/PDF/Example_House.pdf");
#endif
        ExportPDF(outer, path, wallThickness);
        Debug.Log($"PDF exported to: {path}");
    }

    static void ExportPDF(List<Vector2> polygon, string path, float wallThickness)
    {
        usedTextX.Clear();
        usedTextY.Clear();

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

        // Vẽ các dimension line (chiều dài / chiều rộng nhà)
        DrawDimensionLine(cb, new Vector2(0, 0), new Vector2(houseWidth, 0), -300f, scale, shift, offsetX, offsetY, $"{houseWidth} mm");
        DrawDimensionLine(cb, new Vector2(houseWidth, 0), new Vector2(houseWidth, houseHeight), 300f, scale, shift, offsetX, offsetY, $"{houseHeight} mm");
        DrawDimensionLine(cb, new Vector2(houseWidth, houseHeight), new Vector2(0, houseHeight), 300f, scale, shift, offsetX, offsetY, $"{houseWidth} mm");
        DrawDimensionLine(cb, new Vector2(0, houseHeight), new Vector2(0, 0), -300f, scale, shift, offsetX, offsetY, $"{houseHeight} mm");

        // --- DOOR DIMENSIONS (bottom side) ---
        // Vector2 doorCenter = new Vector2(houseWidth / 2f, 0);
        // float doorWidth = 1000f;
        float doorOffset = doorCenter.x - doorWidth / 2f;

        // Khoảng cách từ tường trái đến cửa
        DrawDimensionLine(cb, new Vector2(0, 0), new Vector2(doorOffset, 0), -500f, scale, shift, offsetX, offsetY, $"{doorOffset} mm");

        // Chiều dài cửa
        DrawDimensionLine(cb, new Vector2(doorOffset, 0), new Vector2(doorOffset + doorWidth, 0), -500f, scale, shift, offsetX, offsetY, $"{doorWidth} mm");

        // Khoảng cách từ cửa đến tường phải
        DrawDimensionLine(cb, new Vector2(doorOffset + doorWidth, 0), new Vector2(houseWidth, 0), -500f, scale, shift, offsetX, offsetY, $"{houseWidth - (doorOffset + doorWidth)} mm");


        // --- WINDOW DIMENSIONS (right side) ---
        float winLength = 800f;
        float winOffset = (houseHeight - winLength) / 2f;

        DrawDimensionLine(cb, new Vector2(houseWidth, 0), new Vector2(houseWidth, winOffset), 500f, scale, shift, offsetX, offsetY, $"{winOffset} mm");
        DrawDimensionLine(cb, new Vector2(houseWidth, winOffset), new Vector2(houseWidth, winOffset + winLength), 500f, scale, shift, offsetX, offsetY, $"{winLength} mm");
        DrawDimensionLine(cb, new Vector2(houseWidth, winOffset + winLength), new Vector2(houseWidth, houseHeight), 500f, scale, shift, offsetX, offsetY, $"{houseHeight - (winOffset + winLength)} mm");


        // --- WINDOW DIMENSIONS (top side) ---
        float topWinOffset = (houseWidth - winLength) / 2f;
        DrawDimensionLine(cb, new Vector2(houseWidth, houseHeight), new Vector2(topWinOffset + winLength, houseHeight), 500f, scale, shift, offsetX, offsetY, $"{houseWidth - (topWinOffset + winLength)} mm");
        DrawDimensionLine(cb, new Vector2(topWinOffset, houseHeight), new Vector2(topWinOffset + winLength, houseHeight), 500f, scale, shift, offsetX, offsetY, $"{winLength} mm");
        DrawDimensionLine(cb, new Vector2(0, houseHeight), new Vector2(topWinOffset, houseHeight), 500f, scale, shift, offsetX, offsetY, $"{topWinOffset} mm");


        // --- WINDOW DIMENSIONS (left side) ---
        DrawDimensionLine(cb, new Vector2(0, 0), new Vector2(0, winOffset), -500f, scale, shift, offsetX, offsetY, $"{winOffset} mm");
        DrawDimensionLine(cb, new Vector2(0, winOffset), new Vector2(0, winOffset + winLength), -500f, scale, shift, offsetX, offsetY, $"{winLength} mm");
        DrawDimensionLine(cb, new Vector2(0, winOffset + winLength), new Vector2(0, houseHeight), -500f, scale, shift, offsetX, offsetY, $"{houseHeight - (winOffset + winLength)} mm");

        document.Close();
    }

    //hàm vẽ cửa và cửa sổ
    static void DrawSymbol(PdfContentByte cb, Vector2 center, float angleDeg, float width, float scale, Vector2 shift, float offsetX, float offsetY, string type)
    {
        float half = width / 2f;
        Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        Vector2 p1 = center + dir * -half;
        Vector2 p2 = center + dir * half;

        Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);
        Vector2 cp = Convert(center);
        Vector2 pp1 = Convert(p1);
        Vector2 pp2 = Convert(p2);

        if (type == "door")
        {
            // Cửa xoay: vẽ line (cánh đóng) và cung 90 độ (cung mở)
            cb.SetLineWidth(1f);
            cb.SetRGBColorStroke(0, 0, 255); // Blue

            // Vẽ cánh cửa đóng (line)
            cb.MoveTo(cp.x, cp.y);
            cb.LineTo(pp2.x, pp2.y);
            cb.Stroke();

            // Vẽ cung 90 độ (cung mở) từ trục bản lề
            float radius = (pp2 - cp).magnitude;
            cb.Arc(cp.x - radius, cp.y - radius, cp.x + radius, cp.y + radius,
                -angleDeg, -90); // xoay ngược lại để cùng chiều kim đồng hồ
            cb.Stroke();
        }
        else if (type == "window")
        {
            // Cửa sổ: 2 line song song ngắn
            cb.SetLineWidth(0.5f);
            cb.SetRGBColorStroke(255, 0, 0); // Red

            Vector2 norm = new Vector2(-dir.y, dir.x) * 50f; // chiều dày cửa sổ
            Vector2 winA1 = Convert(p1 + norm);
            Vector2 winA2 = Convert(p1 - norm);
            Vector2 winB1 = Convert(p2 + norm);
            Vector2 winB2 = Convert(p2 - norm);

            cb.MoveTo(winA1.x, winA1.y);
            cb.LineTo(winA2.x, winA2.y);
            cb.MoveTo(winB1.x, winB1.y);
            cb.LineTo(winB2.x, winB2.y);
            cb.Stroke();
        }

        // Reset màu
        cb.SetRGBColorStroke(0, 0, 0);
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
        cb.SetLineWidth(0.25f);
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
        cb.SetFontAndSize(bf, 10);

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
