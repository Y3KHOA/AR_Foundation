using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using System;


public class PdfExporter
{
    // static List<RectangleF> usedTextRects = new List<RectangleF>();
    static List<TextRect> usedTextRects = new List<TextRect>();
    public static Vector2 drawingCenter = Vector2.zero;


    public static byte[] GeneratePdfAsBytes(List<Room> rooms, float wallThickness)
    {
        usedTextRects.Clear();
        HashSet<string> drawnDimensionLines = new HashSet<string>();

        if (rooms == null || rooms.Count == 0) return null;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            PdfContentByte cb = writer.DirectContent;
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            cb.SetFontAndSize(baseFont, 10);

            float wallLineWidth = 1f;
            cb.SetLineWidth(wallLineWidth);
            cb.SetRGBColorStroke(0, 0, 0);

            // === Vẽ chữ ĐÔNG TÂY NAM BẮC quanh trang ===
            cb.SetFontAndSize(baseFont, 32);
            cb.SetRGBColorFill(0, 0, 0); // màu đen

            float pageCenterX = PageSize.A4.Width / 2f;
            float pageCenterY = PageSize.A4.Height / 2f;

            // === Example for N ===
            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "N", pageCenterX, 20, 0);
            cb.EndText();

            // Arc cho N
            float radiusN = 40f;
            float arcXN = pageCenterX - radiusN;
            float arcYN = 20 - radiusN;
            cb.Arc(arcXN, arcYN, arcXN + radiusN * 2, arcYN + radiusN * 2, 0, 180);
            cb.Stroke();

            // === Example for S ===
            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "S", pageCenterX, PageSize.A4.Height - 40, 0);
            cb.EndText();

            float radiusS = 40f;
            float arcXS = pageCenterX - radiusS;
            float arcYS = PageSize.A4.Height - 20 - radiusS;
            // nửa dưới vòng cung
            cb.Arc(arcXS, arcYS, arcXS + radiusS * 2, arcYS + radiusS * 2, 180, 180);
            cb.Stroke();

            // === Tương tự cho E (trái)
            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "E", 40, pageCenterY, 90);
            cb.EndText();

            float radiusE = 40f;
            float arcXE = 20 - radiusE;
            float arcYE = pageCenterY - radiusE;
            cb.Arc(arcXE, arcYE, arcXE + radiusE * 2, arcYE + radiusE * 2, 270, 180);
            cb.Stroke();

            // === W (phải)
            cb.BeginText();
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "W", PageSize.A4.Width - 40, pageCenterY, 270);
            cb.EndText();

            float radiusW = 40f;
            float arcXW = PageSize.A4.Width - 20 - radiusW;
            float arcYW = pageCenterY - radiusW;
            cb.Arc(arcXW, arcYW, arcXW + radiusW * 2, arcYW + radiusW * 2, 90, 180);
            cb.Stroke();

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

            drawingCenter = (globalMin + globalMax) / 2f;
            Debug.Log($"drawingCenter ALL: {drawingCenter}");

            // === TRỪ RA VÙNG CHỮ ĐÔNG TÂY NAM BẮC ===
            float compassMarginTop = 80f;    // Phần trên (S)
            float compassMarginBottom = 100f; // Phần dưới (N)
            float compassMarginLeft = 80f;   // Trái (E)
            float compassMarginRight = 80f;  // Phải (W)

            float availableWidth = PageSize.A4.Width - compassMarginLeft - compassMarginRight;
            float availableHeight = PageSize.A4.Height - compassMarginTop - compassMarginBottom;

            float scale = Mathf.Min(availableWidth / globalSize.x, availableHeight / globalSize.y);

            // Giữ hình vẽ nằm giữa phần còn lại:
            float offsetX = compassMarginLeft + (availableWidth - globalSize.x * scale) / 2f;
            float offsetY = compassMarginBottom + (availableHeight - globalSize.y * scale) / 2f;

            Vector2 shift = -globalMin;

            // === Convert helper ===
            Vector2 Convert(Vector2 pt) => new Vector2((pt.x + shift.x) * scale + offsetX, (pt.y + shift.y) * scale + offsetY);

            // === Vẽ từng Room ===
            foreach (var room in rooms)
            {
                var polygon = room.checkpoints;
                if (polygon.Count < 2) continue;

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

                    // VẼ HATCH TRƯỚC (nằm dưới)
                    Vector2 diagDir = (dir + perp).normalized;
                    Vector2 hatchSpacingDir = new Vector2(-diagDir.y, diagDir.x);

                    List<Vector2> corners = new() { pa, pb, pc, pd };
                    float minProj = float.MaxValue;
                    float maxProj = float.MinValue;
                    foreach (var corner in corners)
                    {
                        float proj = Vector2.Dot(corner, hatchSpacingDir);
                        minProj = Mathf.Min(minProj, proj);
                        maxProj = Mathf.Max(maxProj, proj);
                    }

                    float hatchSpacing = 0.02f;

                    // Đổi nét hatch: nhỏ hơn, màu nhạt
                    cb.SetLineWidth(wallLineWidth * 0.5f);
                    cb.SetRGBColorStroke(150, 150, 150);

                    for (float d = minProj; d <= maxProj; d += hatchSpacing)
                    {
                        Vector2 linePoint = hatchSpacingDir * d;

                        List<Vector2> intersections = new();
                        Vector2 ls = linePoint - diagDir * 1000f;
                        Vector2 le = linePoint + diagDir * 1000f;

                        Vector2[] rectCorners = new Vector2[] { pa, pb, pc, pd };
                        for (int edge = 0; edge < 4; edge++)
                        {
                            Vector2 r1 = rectCorners[edge];
                            Vector2 r2 = rectCorners[(edge + 1) % 4];
                            if (LineSegmentsIntersect(ls, le, r1, r2, out Vector2 ip))
                            {
                                intersections.Add(ip);
                            }
                        }

                        if (intersections.Count == 2)
                        {
                            Vector2 i1 = Convert(intersections[0]);
                            Vector2 i2 = Convert(intersections[1]);
                            cb.MoveTo(i1.x, i1.y);
                            cb.LineTo(i2.x, i2.y);
                            cb.Stroke();
                        }
                    }

                    // Khôi phục nét viền tường
                    cb.SetLineWidth(wallLineWidth);
                    cb.SetRGBColorStroke(0, 0, 0);

                    // VẼ VIỀN TƯỜNG (nằm trên)
                    cb.MoveTo(cpa.x, cpa.y);
                    cb.LineTo(cpb.x, cpb.y);
                    cb.LineTo(cpc.x, cpc.y);
                    cb.LineTo(cpd.x, cpd.y);
                    cb.ClosePath();
                    cb.Stroke();

                    // Đo chiều dài tường
                    Vector2 cp1 = Convert(p1);
                    Vector2 cp2 = Convert(p2);

                    if (!IsAlreadyDrawn(p1, p2, drawnDimensionLines))
                    {
                        DrawDimensionLine(cb, cp1, cp2, -15f, $"{Vector2.Distance(p1, p2):0.00}", drawingCenter,true);
                    }

                    // Đo chiều dày tường
                    // DrawDimensionLine(cb, cpa, cpd, 20f, $"{wallThickness:0.0}");
                }
                // === Vẽ tường thủ công trước, mỏng như tường chính, nhưng màu nhạt hơn để không đè
                foreach (var wall in room.wallLines.Where(w => w.isManualConnection))
                {
                    Vector2 p1 = new Vector2(wall.start.x, wall.start.z);
                    Vector2 p2 = new Vector2(wall.end.x, wall.end.z);

                    Vector2 dir = (p2 - p1).normalized;
                    Vector2 perp = new Vector2(-dir.y, dir.x);
                    Vector2 offset = perp * wallThickness * 0.5f; // bằng tường chính

                    Vector2 pa = p1 + offset;
                    Vector2 pb = p2 + offset;
                    Vector2 pc = p2 - offset;
                    Vector2 pd = p1 - offset;

                    Vector2 cpa = Convert(pa);
                    Vector2 cpb = Convert(pb);
                    Vector2 cpc = Convert(pc);
                    Vector2 cpd = Convert(pd);

                    // HATCH bên trong tường phụ
                    Vector2 diagDir = (dir + perp).normalized;
                    Vector2 hatchSpacingDir = new Vector2(-diagDir.y, diagDir.x);

                    List<Vector2> corners = new() { pa, pb, pc, pd };
                    float minProj = float.MaxValue;
                    float maxProj = float.MinValue;
                    foreach (var corner in corners)
                    {
                        float proj = Vector2.Dot(corner, hatchSpacingDir);
                        minProj = Mathf.Min(minProj, proj);
                        maxProj = Mathf.Max(maxProj, proj);
                    }

                    float hatchSpacing = 0.02f;

                    cb.SetLineWidth(wallLineWidth * 0.5f);
                    cb.SetRGBColorStroke(200, 200, 200); // Màu xám nhạt

                    for (float d = minProj; d <= maxProj; d += hatchSpacing)
                    {
                        Vector2 linePoint = hatchSpacingDir * d;

                        List<Vector2> intersections = new();
                        Vector2 ls = linePoint - diagDir * 1000f;
                        Vector2 le = linePoint + diagDir * 1000f;

                        Vector2[] rectCorners = new Vector2[] { pa, pb, pc, pd };
                        for (int edge = 0; edge < 4; edge++)
                        {
                            Vector2 r1 = rectCorners[edge];
                            Vector2 r2 = rectCorners[(edge + 1) % 4];
                            if (LineSegmentsIntersect(ls, le, r1, r2, out Vector2 ip))
                            {
                                intersections.Add(ip);
                            }
                        }

                        if (intersections.Count == 2)
                        {
                            Vector2 i1 = Convert(intersections[0]);
                            Vector2 i2 = Convert(intersections[1]);
                            cb.MoveTo(i1.x, i1.y);
                            cb.LineTo(i2.x, i2.y);
                            cb.Stroke();
                        }
                    }

                    // Viền tường thủ công
                    cb.SetLineWidth(wallLineWidth);
                    cb.SetRGBColorStroke(100, 100, 100); // viền xám đậm hơn nhưng không đen hẳn

                    cb.MoveTo(cpa.x, cpa.y);
                    cb.LineTo(cpb.x, cpb.y);
                    cb.LineTo(cpc.x, cpc.y);
                    cb.LineTo(cpd.x, cpd.y);
                    cb.ClosePath();
                    cb.Stroke();

                    // Đo chiều dài tường
                    Vector2 cp1 = Convert(p1);
                    Vector2 cp2 = Convert(p2);

                    if (!IsAlreadyDrawn(p1, p2, drawnDimensionLines))
                    {
                        DrawDimensionLine(cb, cp1, cp2, -10f, $"{Vector2.Distance(p1, p2):0.00}", drawingCenter,true);
                    }

                    // Đo chiều dày tường
                    // DrawDimensionLine(cb, cpa, cpd, 20f, $"{wallThickness:0.0}");
                }

                // vẽ point chính
                foreach (var point in polygon)
                {
                    Vector2 cpoint = Convert(point);

                    float boxSize = 0.2f;   // Bạn muốn box 0.2f
                    float halfSize = boxSize * scale * 0.35f; // Phải nhân `scale` vì Convert() đã scale!

                    cb.SetRGBColorFill(0, 0, 0);

                    cb.MoveTo(cpoint.x - halfSize, cpoint.y - halfSize);
                    cb.LineTo(cpoint.x + halfSize, cpoint.y - halfSize);
                    cb.LineTo(cpoint.x + halfSize, cpoint.y + halfSize);
                    cb.LineTo(cpoint.x - halfSize, cpoint.y + halfSize);
                    cb.ClosePath();
                    cb.Fill();
                }
                // === Vẽ các điểm phụ (extraCheckpoints) ===
                foreach (var point in room.extraCheckpoints)
                {
                    Vector2 cpoint = Convert(point);

                    float boxSize = 0.15f;  // nhỏ hơn chút
                    float halfSize = boxSize * scale * 0.35f;

                    cb.SetRGBColorFill(0, 0, 0);

                    cb.MoveTo(cpoint.x - halfSize, cpoint.y - halfSize);
                    cb.LineTo(cpoint.x + halfSize, cpoint.y - halfSize);
                    cb.LineTo(cpoint.x + halfSize, cpoint.y + halfSize);
                    cb.LineTo(cpoint.x - halfSize, cpoint.y + halfSize);
                    cb.ClosePath();
                    cb.Fill();
                }

                // Vẽ cửa và cửa sổ
                foreach (var wall in room.wallLines)
                {
                    if (wall.type == LineType.Door || wall.type == LineType.Window)
                    {
                        // Lấy đúng toạ độ 2D
                        Vector2 start2D = new Vector2(wall.start.x, wall.start.z); // nếu đang lưu Vector3
                        Vector2 end2D = new Vector2(wall.end.x, wall.end.z);

                        // Vector2 startConverted = Convert(start2D);
                        // Vector2 endConverted = Convert(end2D);

                        // DrawSymbol(cb, startConverted, endConverted, wall.type.ToString().ToLower());
                        // Truyền tọa độ gốc, convert nội bộ sau
                        DrawSymbol(cb, Convert, start2D, end2D, wall.type.ToString().ToLower());
                    }
                }

                // Sau khi tính xong shift, scale, offsetX/Y:
                float minX = offsetX;
                float minY = offsetY;
                float maxX = offsetX + globalSize.x * scale;
                float maxY = offsetY + globalSize.y * scale;

                // Gọi hàm vẽ lưới trục (giả sử 5 cột, 5 hàng, khoảng cách 100f)
                // DrawGridLines(cb, 100f, 100f, 6, 6, minX, minY, maxX, maxY);
            }

            // === Vẽ khung thông tin Room ===
            float infoBoxWidth = 150f;
            float infoBoxHeight = 70f;

            float boxX = PageSize.A4.Width - infoBoxWidth - 20f; // Right padding
            float boxY = 20f; // Bottom padding

            // Nền
            cb.SetLineWidth(0.5f);
            cb.SetRGBColorStroke(0, 0, 0);
            cb.SetRGBColorFill(255, 255, 255);

            cb.Rectangle(boxX, boxY, infoBoxWidth, infoBoxHeight);
            cb.FillStroke();

            // Text
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 10);
            cb.SetRGBColorFill(0, 0, 0);

            // Tính số liệu
            float totalLength = 0f;
            float totalWidth = 0f;
            float totalPerimeter = 0f;
            float totalArea = 0f;

            foreach (var room in rooms)
            {
                var pts = room.checkpoints;
                if (pts == null || pts.Count < 3) continue;

                float perimeter = 0f;
                float maxLength = 0f;
                float minLength = float.MaxValue;
                float area = 0f;

                for (int i = 0; i < pts.Count; i++)
                {
                    Vector2 a = pts[i];
                    Vector2 b = pts[(i + 1) % pts.Count];
                    float dist = Vector2.Distance(a, b);
                    perimeter += dist;
                    maxLength = Mathf.Max(maxLength, dist);
                    minLength = Mathf.Min(minLength, dist);
                    area += (a.x * b.y - b.x * a.y);
                }

                area = Mathf.Abs(area) * 0.5f;

                totalLength += maxLength;
                totalWidth += minLength;
                totalPerimeter += perimeter;
                totalArea += area;
            }

            float textX = boxX + 5f;
            float textY = boxY + infoBoxHeight - 12f;
            float leading = 12f; // Khoảng cách dòng

            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Thong so ban ve:", textX, textY, 0);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Chieu dai: {totalLength:0.00} m", textX, textY - leading, 0);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Chieu rong: {totalWidth:0.00} m", textX, textY - 2 * leading, 0);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Chu vi: {totalPerimeter:0.00} m", textX, textY - 3 * leading, 0);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Dien tich: {totalArea:0.00} m²", textX, textY - 4 * leading, 0);

            cb.EndText();

            document.Close();
            return memoryStream.ToArray();
        }
    }

    static Vector2 Round(Vector2 v, int decimals = 3)
{
    return new Vector2((float)Math.Round(v.x, decimals), (float)Math.Round(v.y, decimals));
}

static bool IsAlreadyDrawn(Vector2 a, Vector2 b, HashSet<string> drawnSet)
{
    Vector2 ra = Round(a);
    Vector2 rb = Round(b);
    string k1 = $"{ra.x}_{ra.y}_{rb.x}_{rb.y}";
    string k2 = $"{rb.x}_{rb.y}_{ra.x}_{ra.y}";
    if (drawnSet.Contains(k1) || drawnSet.Contains(k2)) return true;
    drawnSet.Add(k1);
    return false;
}
    
    static bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;
        float rxs = r.x * s.y - r.y * s.x;
        float qpxr = (q1 - p1).x * r.y - (q1 - p1).y * r.x;

        if (Mathf.Abs(rxs) < 1e-8f) return false; // song song

        float t = ((q1 - p1).x * s.y - (q1 - p1).y * s.x) / rxs;
        float u = ((q1 - p1).x * r.y - (q1 - p1).y * r.x) / rxs;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            intersection = p1 + t * r;
            return true;
        }
        return false;
    }

    //hàm vẽ cửa và cửa sổ
    static void DrawSymbol(PdfContentByte cb, System.Func<Vector2, Vector2> Convert, Vector2 p1, Vector2 p2, string type)
    {
        if (type == "door")
        {
            float DoorLineWidth = 1f;
            cb.SetLineWidth(DoorLineWidth);
            cb.SetRGBColorStroke(0, 0, 0); // Blue
            cb.SetRGBColorFill(255, 255, 255); // Fill trắng

            float radius = Vector2.Distance(p1, p2);
            float angleStart = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;

            // 1) Cánh cửa
            Vector2 dir = (p2 - p1).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            float rectWidth = 0.1f; // bề dày thực tế

            Vector2 offset = normal * (rectWidth * 0.5f);

            Vector2 pa = p1 + offset;
            Vector2 pb = p2 + offset;
            Vector2 pc = p2 - offset;
            Vector2 pd = p1 - offset;

            Vector2 cpa = Convert(pa);
            Vector2 cpb = Convert(pb);
            Vector2 cpc = Convert(pc);
            Vector2 cpd = Convert(pd);

            cb.MoveTo(cpa.x, cpa.y);
            cb.LineTo(cpb.x, cpb.y);
            cb.LineTo(cpc.x, cpc.y);
            cb.LineTo(cpd.x, cpd.y);
            cb.ClosePath();
            cb.FillStroke(); // Hoặc cb.Stroke() nếu chỉ cần viền

            // 2) Vẽ cung 90° bằng các đoạn line nhỏ
            cb.SetLineWidth(0.5f); // Độ dày riêng cho cung
            cb.SetLineDash(3f, 3f); // Bật nét đứt: nét dài 3, đứt 3

            int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float a1 = (angleStart + i * (90f / segments)) * Mathf.Deg2Rad;
                float a2 = (angleStart + (i + 1) * (90f / segments)) * Mathf.Deg2Rad;

                Vector2 pA = new Vector2(p1.x + radius * Mathf.Cos(a1), p1.y + radius * Mathf.Sin(a1));
                Vector2 pB = new Vector2(p1.x + radius * Mathf.Cos(a2), p1.y + radius * Mathf.Sin(a2));

                Vector2 cpA = Convert(pA);
                Vector2 cpB = Convert(pB);

                cb.MoveTo(cpA.x, cpA.y);
                cb.LineTo(cpB.x, cpB.y);
                cb.Stroke();
            }

            cb.SetLineDash(0f); // Quay về nét liền cho phần tiếp theo

            // 3) Tính điểm đầu & cuối cung
            float radStart = angleStart * Mathf.Deg2Rad;
            float radEnd = (angleStart + 90) * Mathf.Deg2Rad;

            Vector2 arcStart = new Vector2(p1.x + radius * Mathf.Cos(radStart), p1.y + radius * Mathf.Sin(radStart));
            Vector2 arcEnd = new Vector2(p1.x + radius * Mathf.Cos(radEnd), p1.y + radius * Mathf.Sin(radEnd));

            // Biên đầu
            Vector2 radiusVecStart = (arcStart - p1).normalized;
            Vector2 tangentStart = new Vector2(-radiusVecStart.y, radiusVecStart.x);

            cb.MoveTo(Convert(arcStart).x, Convert(arcStart).y);
            cb.LineTo(Convert(arcStart + tangentStart * (radius * 0.05f)).x,
                    Convert(arcStart + tangentStart * (radius * 0.05f)).y);
            cb.Stroke();

            // Biên cuối
            Vector2 radiusVecEnd = (arcEnd - p1).normalized;
            Vector2 tangentEnd = new Vector2(-radiusVecEnd.y, radiusVecEnd.x);

            cb.MoveTo(Convert(arcEnd).x, Convert(arcEnd).y);
            cb.LineTo(Convert(arcEnd - tangentEnd * (radius * 0.05f)).x,
                    Convert(arcEnd - tangentEnd * (radius * 0.05f)).y);
            cb.Stroke();

            // Đường nối cuối cung về bản lề
            cb.SetLineWidth(DoorLineWidth);
            cb.MoveTo(Convert(arcEnd).x, Convert(arcEnd).y);
            cb.LineTo(Convert(p1).x, Convert(p1).y);
            cb.Stroke();

            // === 4) Đo khoảng mở cửa ===
            cb.SetRGBColorFill(0, 0, 0); // reset color
            Vector2 cp1 = Convert(p1);
            Vector2 cp2 = Convert(p2);

            float doorLength = Vector2.Distance(p1, p2);
            string doorLabel = $"{doorLength:0.00}";

            DrawDimensionLine(cb, cp1, cp2, -5f, doorLabel, drawingCenter,false);
        }

        else if (type == "window")
        {
            float DoorLineWidth = 1f;
            cb.SetLineWidth(DoorLineWidth);
            cb.SetRGBColorStroke(0, 0, 0); // Blue
            cb.SetRGBColorFill(255, 255, 255); // Fill trắng

            // 1) Cánh cửa
            Vector2 dir = (p2 - p1).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            float rectWidth = 0.1f; // bề dày thực tế

            Vector2 offset = normal * (rectWidth * 0.5f);

            Vector2 pa = p1 + offset;
            Vector2 pb = p2 + offset;
            Vector2 pc = p2 - offset;
            Vector2 pd = p1 - offset;

            Vector2 cpa = Convert(pa);
            Vector2 cpb = Convert(pb);
            Vector2 cpc = Convert(pc);
            Vector2 cpd = Convert(pd);

            cb.MoveTo(cpa.x, cpa.y);
            cb.LineTo(cpb.x, cpb.y);
            cb.LineTo(cpc.x, cpc.y);
            cb.LineTo(cpd.x, cpd.y);
            cb.ClosePath();
            cb.FillStroke(); // Hoặc cb.Stroke() nếu chỉ cần viền

            // === Vẽ nét giữa ===
            cb.SetLineWidth(0.5f); // mảnh
            cb.MoveTo(Convert(p1).x, Convert(p1).y);
            cb.LineTo(Convert(p2).x, Convert(p2).y);
            cb.Stroke();

            // === Đo kích thước ===
            cb.SetRGBColorFill(0, 0, 0); // reset color
            Vector2 cp1 = Convert(p1);
            Vector2 cp2 = Convert(p2);

            float doorLength = Vector2.Distance(p1, p2);
            string doorLabel = $"{doorLength:0.00}";

            DrawDimensionLine(cb, cp1, cp2, -5f, doorLabel, drawingCenter,false);
        }

        cb.SetRGBColorStroke(0, 0, 0); // Reset màu
    }

    // public static Vector2 drawingCenter;

    static void DrawDimensionLine(PdfContentByte cb, Vector2 p1, Vector2 p2, float offsetDistance, string label, Vector2 drawingCenter, bool isFixedOffset)
    {
        Vector2 dir = (p2 - p1).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x); // vector vuông góc

        Vector2 mid = (p1 + p2) * 0.5f;
        Vector2 toCenter = (mid - drawingCenter).normalized;

        if ((drawingCenter - mid).y < 0)
            perp *= -1;

        if (Mathf.Abs(offsetDistance) < 0.01f)
            offsetDistance = Mathf.Clamp(Vector2.Distance(p1, p2) * 0.08f, 20f, 40f);

        Vector2 p1Offset = p1 + perp * offsetDistance;
        Vector2 p2Offset = p2 + perp * offsetDistance;

        // === Vẽ đường kích thước ===
        cb.SetLineWidth(0.5f);
        cb.MoveTo(p1Offset.x, p1Offset.y);
        cb.LineTo(p2Offset.x, p2Offset.y);
        cb.Stroke();

        // === Vẽ đường gióng ===
        cb.MoveTo(p1.x, p1.y); cb.LineTo(p1Offset.x, p1Offset.y);
        cb.MoveTo(p2.x, p2.y); cb.LineTo(p2Offset.x, p2Offset.y);
        cb.Stroke();

        // === Mũi tên ===
        float arrowSize = 3f;
        void DrawArrow(Vector2 pos, Vector2 direction)
        {
            Vector2 dir2 = -direction.normalized;
            Vector2 left = pos - dir2 * arrowSize + new Vector2(-dir2.y, dir2.x) * (arrowSize * 0.5f);
            Vector2 right = pos - dir2 * arrowSize - new Vector2(-dir2.y, dir2.x) * (arrowSize * 0.5f);
            cb.MoveTo(left.x, left.y); cb.LineTo(pos.x, pos.y); cb.LineTo(right.x, right.y);
            cb.Stroke();
        }
        DrawArrow(p1Offset, dir);
        DrawArrow(p2Offset, -dir);

        // === Vẽ text ===
        Vector2 midPoint = (p1Offset + p2Offset) * 0.5f;
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.BeginText();
        cb.SetFontAndSize(bf, 7);

        float angle = Mathf.Atan2(perp.y, perp.x) * Mathf.Rad2Deg - 90f;
        Vector2 textToCenter = (drawingCenter - midPoint).normalized;
        if (Vector2.Dot(perp, textToCenter) > 0)
            angle += 180f;

        float textWidth = bf.GetWidthPoint(label, 7);
        float textHeight = 7f;

        Vector2 finalMidPoint = midPoint;
        TextRect labelRect;

        if (!isFixedOffset)
        {
            int tryCount = 0;
            int maxTries = 20;
            float wallClearance = 4f;      
            float spacingStep = 1.5f;      // Đẩy xa đều hơn
            float paddingMargin = 2f;      // Mở rộng vùng né text


            Vector2 initialMidPoint = midPoint;
            Vector2 bestMidPoint = midPoint;
            float bestDist = float.MinValue;
            bool success = false;

            Vector2 perpOriginal = perp; // ← lưu lại hướng gốc

            for (int pass = 0; pass < 2; pass++)
            {
                tryCount = 0;
                perp = (pass == 0) ? perpOriginal : -perpOriginal; // dùng lại hướng đúng
                midPoint = initialMidPoint;

                while (tryCount++ < maxTries)
                {
                    midPoint = initialMidPoint + perp * (wallClearance + tryCount * spacingStep);

                    labelRect = new TextRect(
                        midPoint.x - textWidth / 2f - paddingMargin,
                        midPoint.y - textHeight / 2f - paddingMargin,
                        textWidth + 2 * paddingMargin,
                        textHeight + 2 * paddingMargin
                    );

                    bool overlapsText = usedTextRects.Any(r => r.Intersects(labelRect));
                    bool tooCloseToWall = IsTextTooCloseToLine(labelRect, p1, p2);

                    if (!overlapsText && !tooCloseToWall)
                    {
                        finalMidPoint = midPoint;
                        success = true;
                        break;
                    }

                    Vector2 center = new Vector2(labelRect.x + labelRect.width / 2f, labelRect.y + labelRect.height / 2f);
                    Vector2 closest = ClosestPointOnSegment(p1, p2, center);
                    float dist = Vector2.Distance(center, closest);

                    if (dist > bestDist)
                    {
                        bestDist = dist;
                        bestMidPoint = midPoint;
                    }
                }

                if (success) break;
            }

            if (!success)
                finalMidPoint = bestMidPoint;
        }

        labelRect = new TextRect(
            finalMidPoint.x - textWidth / 2f,
            finalMidPoint.y - textHeight / 2f,
            textWidth,
            textHeight
        );

        usedTextRects.Add(labelRect);
        cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, label, finalMidPoint.x, finalMidPoint.y, angle);
        cb.EndText();
    }

    static bool IsTextTooCloseToLine(TextRect rect, Vector2 lineStart, Vector2 lineEnd)
    {
        float padding = 5f; // ngưỡng khoảng cách tối thiểu cần tránh tường

        // Tính tâm chữ
        Vector2 textCenter = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);

        // Tính khoảng cách từ textCenter đến đoạn thẳng
        Vector2 closest = ClosestPointOnSegment(lineStart, lineEnd, textCenter);
        float dist = Vector2.Distance(textCenter, closest);

        return dist < padding;
    }

    static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    static void DrawGridLines(PdfContentByte cb, float spacingX, float spacingY, int countX, int countY,
                    float minX, float minY, float maxX, float maxY)
    {
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);

        // === Vẽ trục dọc (số) ===
        for (int i = 0; i < countX; i++)
        {
            float x = minX + i * spacingX;
            cb.MoveTo(x, minY);
            cb.LineTo(x, maxY);
            cb.Stroke();

            // Vẽ bubble
            float bubbleRadius = 10f;  // Tùy scale
            float labelY = maxY + bubbleRadius * 2;

            cb.Circle(x, labelY, bubbleRadius);
            cb.Stroke();

            // Vẽ số bên trong bubble
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, $"{i + 1}", x, labelY - 3f, 0);
            cb.EndText();
        }

        // === Vẽ trục ngang (chữ) ===
        for (int j = 0; j < countY; j++)
        {
            float y = minY + j * spacingY;
            cb.MoveTo(minX, y);
            cb.LineTo(maxX, y);
            cb.Stroke();

            // Vẽ bubble
            float bubbleRadius = 10f;
            float labelX = minX - bubbleRadius * 2;

            cb.Circle(labelX, y, bubbleRadius);
            cb.Stroke();

            // Vẽ chữ bên trong bubble (A, B, C, ...)
            char letter = (char)('A' + j);
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, $"{letter}", labelX, y - 3f, 0);
            cb.EndText();
        }
    }
}