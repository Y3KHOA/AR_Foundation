using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadManager
{
    // private static string savePath = Path.Combine(Application.persistentDataPath, "Data/savedData.json");
    private static string savePath = Path.Combine("/storage/emulated/0/Download", "XHeroScan/Data/Drawing_All_Test1.json");

    public static void Save()
    {
        List<List<Vector2>> points = DataTransfer.Instance.GetAllPoints();
        List<List<float>> heights = DataTransfer.Instance.GetAllHeights();

        float Area = DataTransfer.Instance.AreaValue;
        float Perimeter = DataTransfer.Instance.PerimeterValue;
        float Ceiling = DataTransfer.Instance.CeilingValue;

        SaveData saveData = new SaveData
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            area = Area,
            perimeter = Perimeter,
            ceiling = Ceiling
        };

        for (int i = 0; i < points.Count; i++)
        {
            SavedPath path = new SavedPath();
            path.points = new List<Vector2Serializable>();
            path.heights = new List<float>();

            for (int j = 0; j < points[i].Count; j++)
            {
                path.points.Add(new Vector2Serializable(points[i][j]));
                path.heights.Add(heights[i][j]);
            }

            saveData.paths.Add(path);
        }

        string json = JsonUtility.ToJson(saveData, true);

        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(savePath, json);
        Debug.Log("[Save] Đã lưu dữ liệu vào: " + savePath);
    }


    public static void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("[Load] Không tìm thấy file lưu.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        DataTransfer.Instance.AreaValue = saveData.area;
        DataTransfer.Instance.PerimeterValue = saveData.perimeter;
        DataTransfer.Instance.CeilingValue = saveData.ceiling;

        List<List<Vector2>> loadedPoints = new List<List<Vector2>>();
        List<List<float>> loadedHeights = new List<List<float>>();

        foreach (var path in saveData.paths)
        {
            List<Vector2> path2D = new List<Vector2>();
            foreach (var v in path.points)
            {
                path2D.Add(v.ToVector2());
            }
            loadedPoints.Add(path2D);
            loadedHeights.Add(new List<float>(path.heights));
        }

        DataTransfer.Instance.SetAllPoints(loadedPoints);
        DataTransfer.Instance.SetAllHeights(loadedHeights);
        Debug.Log("[Load] with data " + loadedPoints.Count + " points.");

        SceneManager.LoadScene("FlatExampleScene");
    }
}
