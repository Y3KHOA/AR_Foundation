using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryUtils
{
    // Phát hiện các vòng kín từ danh sách cạnh (edges)
    public static List<List<Vector2>> FindClosedPolygons(List<(Vector2, Vector2)> edges)
    {
        Dictionary<Vector2, HashSet<Vector2>> graph = new();
        foreach (var (a, b) in edges)
        {
            if (Vector2.Distance(a, b) < 0.001f) continue;

            if (!graph.ContainsKey(a)) graph[a] = new();
            if (!graph.ContainsKey(b)) graph[b] = new();
            graph[a].Add(b);
            graph[b].Add(a);
        }

        HashSet<string> uniqueLoops = new();
        List<List<Vector2>> loops = new();

        foreach (var start in graph.Keys)
        {
            Stack<Vector2> path = new();
            HashSet<Vector2> visited = new();
            DFS(start, start, graph, path, visited, uniqueLoops, loops);
        }

        return loops;
    }

    private static void DFS(Vector2 current, Vector2 target, Dictionary<Vector2, HashSet<Vector2>> graph,
        Stack<Vector2> path, HashSet<Vector2> visited, HashSet<string> uniqueLoops, List<List<Vector2>> loops)
    {
        path.Push(current);
        visited.Add(current);

        foreach (var neighbor in graph[current])
        {
            if (neighbor == target && path.Count >= 3)
            {
                var cycle = path.Reverse().ToList();
                cycle.Add(target); // khép kín

                // Bỏ nếu số điểm phân biệt quá ít (vd: vòng lặp quanh 1 cạnh)
                // if (cycle.Distinct().Count() < 4) continue;
                if (HasRepeatedEdges(cycle)) continue;


                if (!IsDuplicateLoop(cycle, uniqueLoops) && IsPolygonClosed(cycle))
                {
                    loops.Add(cycle);
                    Debug.Log($"[SSSSSSS]→ Vòng thực sự: {string.Join(" -> ", cycle.Select(p => $"({p.x:F1},{p.y:F1})"))}");
                }
            }
            else if (!visited.Contains(neighbor))
            {
                DFS(neighbor, target, graph, path, visited, uniqueLoops, loops);
            }
        }

        path.Pop();
        visited.Remove(current);
    }

    private static bool IsDuplicateLoop(List<Vector2> inputLoop, HashSet<string> known)
    {
        if (inputLoop.Count < 4) return true;

        // Chuẩn hóa bỏ điểm cuối nếu giống điểm đầu
        // bool isClosed = Vector2.Distance(inputLoop[0], inputLoop[^1]) < 0.001f;
        // var loop = isClosed ? inputLoop.Take(inputLoop.Count - 1).ToList() : inputLoop;
        // Luôn tạo bản sao đã xử lý điểm trùng đầu-cuối
        List<Vector2> loop = Vector2.Distance(inputLoop[0], inputLoop[^1]) < 0.001f
        ? inputLoop.Take(inputLoop.Count - 1).ToList()
        : new List<Vector2>(inputLoop);

        Vector2 minPoint = loop.Aggregate((min, p) =>
            (p.x < min.x || (Mathf.Approximately(p.x, min.x) && p.y < min.y)) ? p : min);

        int minIndex = loop.IndexOf(minPoint);
        var ordered = loop.Skip(minIndex).Concat(loop.Take(minIndex)).ToList();
        var reversed = ordered.AsEnumerable().Reverse().ToList();

        string hash1 = string.Join("-", ordered.Select(p => $"{p.x:F3},{p.y:F3}"));
        string hash2 = string.Join("-", reversed.Select(p => $"{p.x:F3},{p.y:F3}"));

        if (known.Contains(hash1) || known.Contains(hash2)) return true;

        known.Add(hash1);
        return false;
    }

    public static bool ArePolygonsEqual(List<Vector2> a, List<Vector2> b, float tolerance = 0.01f)
    {
        if (a.Count != b.Count) return false;

        int n = a.Count;

        for (int offset = 0; offset < n; offset++)
        {
            bool matchCW = true;
            bool matchCCW = true;

            for (int i = 0; i < n; i++)
            {
                if (Vector2.Distance(a[i], b[(i + offset) % n]) > tolerance)
                    matchCW = false;
                if (Vector2.Distance(a[i], b[(n - offset + i) % n]) > tolerance)
                    matchCCW = false;
            }

            if (matchCW || matchCCW) return true;
        }

        return false;
    }

    private static bool IsPolygonClosed(List<Vector2> loop, float tolerance = 0.01f)
    {
        if (loop.Count < 4) return false;
        return Vector2.Distance(loop[0], loop[^1]) < tolerance;
    }
    private static bool HasRepeatedEdges(List<Vector2> loop)
{
    var edgeSet = new HashSet<string>();
    for (int i = 0; i < loop.Count - 1; i++)
    {
        var a = loop[i];
        var b = loop[i + 1];
        string key1 = $"{a.x:F3},{a.y:F3}|{b.x:F3},{b.y:F3}";
        string key2 = $"{b.x:F3},{b.y:F3}|{a.x:F3},{a.y:F3}";

        // Nếu đã có cạnh này (bất kể chiều), return true
        if (edgeSet.Contains(key1) || edgeSet.Contains(key2))
            return true;

        edgeSet.Add(key1);
    }
    return false;
}

}
