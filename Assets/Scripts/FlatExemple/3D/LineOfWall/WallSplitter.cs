using System.Collections.Generic;
using UnityEngine;

public static class WallSplitter
{
    public static List<WallLine> SplitWall(WallLine originalWall, List<InsertItem> inserts, float defaultWallHeight)
    {
        List<WallLine> result = new List<WallLine>();

        // Tính toán chiều dài đoạn gốc và hướng
        Vector3 start = originalWall.start;
        Vector3 end = originalWall.end;
        Vector3 dir = (end - start).normalized;
        float totalLength = Vector3.Distance(start, end);

        // Tính khoảng cách theo tỷ lệ dọc theo đoạn gốc
        List<(float t, InsertItem item)> insertPoints = new List<(float, InsertItem)>();
        foreach (var item in inserts)
        {
            Vector3 midpoint = (item.start + item.end) / 2f;
            Vector3 projected = ProjectPointOnLineSegment(start, end, midpoint);
            float t = Vector3.Dot(projected - start, dir) / totalLength;
            insertPoints.Add((t, item));
        }

        // Sắp xếp theo t
        insertPoints.Sort((a, b) => a.t.CompareTo(b.t));

        Vector3 last = start;
        foreach (var (t, item) in insertPoints)
        {
            Vector3 segStart = item.start;
            Vector3 segEnd = item.end;

            // 1. Đoạn Wall trước insert (nếu có)
            if (Vector3.Distance(last, segStart) > 0.01f)
            {
                result.Add(new WallLine(last, segStart, LineType.Wall, 0f, defaultWallHeight));
            }

            // 2. Insert đoạn Door/Window
            result.Add(new WallLine(segStart, segEnd, item.type, segStart.y, item.height));

            last = segEnd;
        }

        // 3. Đoạn Wall sau cùng
        if (Vector3.Distance(last, end) > 0.01f)
        {
            result.Add(new WallLine(last, end, LineType.Wall, 0f, defaultWallHeight));
        }

        return result;
    }

    private static Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
}
