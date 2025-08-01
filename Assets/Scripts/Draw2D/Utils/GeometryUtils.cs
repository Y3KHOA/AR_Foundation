using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryUtils
{
    // 1. Xây graph
    static void BuildGraph(Room room,
                           out Dictionary<Vector2, List<Vector2>> adj,
                           out int edgeCount)
    {
        adj = new Dictionary<Vector2, List<Vector2>>();
        edgeCount = 0;

        foreach (var wall in room.wallLines)
        {
            if (wall.type != LineType.Wall && !wall.isManualConnection) continue;

            Vector2 a = new Vector2(wall.start.x, wall.start.z);
            Vector2 b = new Vector2(wall.end.x, wall.end.z);
            if (Vector2.Distance(a, b) < 0.001f) continue;

            if (!adj.ContainsKey(a)) adj[a] = new List<Vector2>();
            if (!adj.ContainsKey(b)) adj[b] = new List<Vector2>();

            if (!adj[a].Contains(b))          // tránh thêm trùng
            {
                adj[a].Add(b);
                adj[b].Add(a);
                edgeCount++;
            }
        }
    }

    // 2. Đếm thành phần liên thông (Euler) 
    static int CountComponents(Dictionary<Vector2, List<Vector2>> adj)
    {
        HashSet<Vector2> seen = new();
        int comp = 0;
        foreach (var v in adj.Keys)
        {
            if (seen.Contains(v)) continue;
            comp++;

            var st = new Stack<Vector2>();
            st.Push(v); seen.Add(v);

            while (st.Count > 0)
            {
                var cur = st.Pop();
                foreach (var nb in adj[cur])
                    if (!seen.Contains(nb)) { seen.Add(nb); st.Push(nb); }
            }
        }
        return comp;
    }

    // 3. Đếm nhanh bằng Euler
    public static int CountLoopsInRoom(Room room)
    {
        if (room == null) return 0;
        BuildGraph(room, out var adj, out int E);
        int V = adj.Count;
        int C = CountComponents(adj);
        ListLoopsInRoom(room);
        return Math.Max(E - V + C, 0);
    }

    // 4. Liệt kê tất cả loop và log ra

    public static List<List<Vector2>> ListLoopsInRoom(Room room)
    {
        if (room == null) return new();

        BuildGraph(room, out var adj, out _);

        // dùng thuật toán “face-tracing” đơn giản dựa trên left-hand rule
        HashSet<string> known = new();         // tránh trùng
        List<List<Vector2>> loops = new();
        HashSet<(Vector2, Vector2)> usedDir = new();   // <từ, tới> đã duyệt

        foreach (var v in adj.Keys)
            foreach (var n in adj[v])
            {
                if (usedDir.Contains((v, n))) continue;

                List<Vector2> loop = new() { v };
                Vector2 prev = v, cur = n;

                while (true)
                {
                    loop.Add(cur);
                    usedDir.Add((prev, cur));

                    // tìm “hàng xóm bên trái nhất” (left-hand rule) của cạnh prev→cur
                    Vector2 next = adj[cur]
                        .Where(nb => nb != prev)
                        .OrderBy(nb => LeftTurnAngle(prev, cur, nb))
                        .FirstOrDefault();

                    if (next == default) break;
                    if (next == loop[0])
                    {
                        // khép vòng
                        loop.Add(next);
                        if (IsSimpleLoop(loop) && AddIfNew(loop, known, loops))
                            Debug.Log($"[DEBUG][LOOP] {room.ID}  ⟹  {string.Join(" → ", loop.Select(p => p.ToString()))}");
                        break;
                    }

                    prev = cur;
                    cur = next;

                    // quá dài ⇒ an toàn thoát
                    if (loop.Count > 1000) break;
                }
            }
        Debug.Log($">>> Phòng {room.ID} có {loops.Count} loop được liệt kê.");
        return loops;
    }

    static float LeftTurnAngle(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v1 = (a - b).normalized;
        Vector2 v2 = (c - b).normalized;
        float angle = Vector2.SignedAngle(v1, v2);
        return (angle < 0) ? angle + 360f : angle; // 0..360 (trái nhỏ nhất)
    }

    static bool IsSimpleLoop(List<Vector2> loop)
    {
        if (loop.Count < 4) return false; // 3 cạnh + quay về đầu
        // bỏ điểm cuối
        var pts = loop.Take(loop.Count - 1).ToList();
        // không trùng đỉnh
        return pts.Distinct().Count() == pts.Count;
    }

    static bool AddIfNew(List<Vector2> rawLoop,
                         HashSet<string> known,
                         List<List<Vector2>> loops)
    {
        // chuẩn hoá: bỏ điểm cuối, xoay sao cho điểm min đầu, lấy 2 chiều
        var loop = rawLoop.Take(rawLoop.Count - 1).ToList();
        Vector2 min = loop.Aggregate((x, y) => (x.x < y.x || (Mathf.Approximately(x.x, y.x) && x.y < y.y)) ? x : y);
        int idx = loop.IndexOf(min);
        var cw = loop.Skip(idx).Concat(loop.Take(idx)).ToList();
        var ccw = cw.AsEnumerable().Reverse().ToList();

        string h1 = string.Join("|", cw);
        string h2 = string.Join("|", ccw);

        string key = string.CompareOrdinal(h1, h2) < 0 ? h1 : h2;
        if (known.Contains(key)) return false;

        known.Add(key);
        loops.Add(loop);
        return true;
    }
    public static bool EdgeInLoop(List<Vector2> loop, Vector2 a, Vector2 b)
    {
        for (int i = 0; i < loop.Count; i++)
        {
            Vector2 p1 = loop[i];
            Vector2 p2 = loop[(i + 1) % loop.Count];

            if ((Vector2.Distance(p1, a) < 0.001f && Vector2.Distance(p2, b) < 0.001f) ||
                (Vector2.Distance(p1, b) < 0.001f && Vector2.Distance(p2, a) < 0.001f))
                return true;
        }
        return false;
    }
    public static bool PointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int crossings = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            if (((a.y > point.y) != (b.y > point.y)) &&
                 (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y + 1e-6f) + a.x))
                crossings++;
        }
        return (crossings % 2 == 1);
    }

    // ✨ Thêm vào GeometryUtils (nếu chưa có)
    public static Vector2 GetCentroid(List<Vector2> poly)
    {
        float cx = 0, cy = 0;
        foreach (var p in poly) { cx += p.x; cy += p.y; }
        return new Vector2(cx / poly.Count, cy / poly.Count);
    }
    public static bool IsSamePolygon(List<Vector2> a, List<Vector2> b, float tolerance = 0.001f)
    {
        if (a.Count != b.Count) return false;

        int startIndex = -1;
        for (int i = 0; i < b.Count; i++)
        {
            if (Vector2.Distance(a[0], b[i]) < tolerance)
            {
                startIndex = i;
                break;
            }
        }
        if (startIndex == -1) return false;

        for (int i = 0; i < a.Count; i++)
        {
            Vector2 aPt = a[i];
            Vector2 bPt = b[(startIndex + i) % b.Count];
            if (Vector2.Distance(aPt, bPt) > tolerance)
                return false;
        }

        return true;
    }
    public static float ComputePolygonArea(List<Vector2> polygon)
    {
        float area = 0;
        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % n];
            area += (p1.x * p2.y - p2.x * p1.y);
        }
        return Mathf.Abs(area) * 0.5f;
    }
    public static bool IsSamePolygonFlexible(List<Vector2> a, List<Vector2> b, float tol = 0.001f)
    {
        if (a.Count != b.Count) return false;

        // Thử 2 chiều: thuận và đảo
        for (int dir = 0; dir < 2; dir++)
        {
            bool ok = true;
            // tìm offset khớp đỉnh đầu
            int start = -1;
            for (int i = 0; i < b.Count; i++)
            {
                Vector2 bp = (dir == 0) ? b[i] : b[b.Count - 1 - i];
                if (Vector2.Distance(a[0], bp) < tol) { start = i; break; }
            }
            if (start == -1) ok = false;
            else
            {
                for (int k = 0; k < a.Count && ok; k++)
                {
                    Vector2 ap = a[k];
                    int idx = (dir == 0)
                              ? (start + k) % b.Count
                              : (start - k + b.Count) % b.Count;
                    Vector2 bp = b[idx];
                    if (Vector2.Distance(ap, bp) > tol) ok = false;
                }
            }
            if (ok) return true;
        }
        return false;
    }
    public static float AbsArea(List<Vector2> poly)
    {
        double area = 0;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            area += (double)poly[j].x * poly[i].y - (double)poly[i].x * poly[j].y;
        return Mathf.Abs((float)(area * 0.5));
    }
    public static bool PolygonInsidePolygon(List<Vector2> inner, List<Vector2> outer)
{
    foreach (var p in inner)
    {
        if (!PointInPolygon(p, outer))
            return false;
    }
    return true;
}


}
