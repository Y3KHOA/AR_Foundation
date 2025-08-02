using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadManager
{
    public static string saveName = "DrawingData";

    public static void Save(string customName = null)
    {
        SaveData saveData = new SaveData
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            paths = new List<SavedPath>()
        };

        if (DoesNameExist(customName))
        {
            Debug.LogWarning("Tên đã tồn tại. Hãy chọn tên khác.");
            return;
        }

        foreach (Room room in RoomStorage.rooms)
        {
            var path = new SavedPath
            {
                roomID = room.ID,
                points = room.checkpoints.ConvertAll(p => new Vector2Serializable(p)),
                heights = new List<float>(room.heights),
                wallLines = room.wallLines.ConvertAll(w => new SavedWallLine
                {
                    start = w.start,
                    end = w.end,
                    type = w.type,
                    distanceHeight = w.distanceHeight,
                    Height = w.Height,
                    isManualConnection = w.isManualConnection
                }),
                compass = new Vector2Serializable(room.Compass),
                headingCompass = room.headingCompass
            };
            saveData.paths.Add(path);
        }

        // Tạo timestamp phù hợp cho tên file
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Dùng custom name nếu có, ngược lại dùng mặc định
        string baseName = string.IsNullOrEmpty(customName) ? saveName : customName;
        string fileName = $"{baseName}.json";

        string pathToSave = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(pathToSave, JsonUtility.ToJson(saveData, true));

        Debug.Log($"[Save] OK: {pathToSave}");
    }

    public static void Load(string fileName = "DrawingData.json")
    {
        string pathToLoad = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(pathToLoad))
        {
            Debug.LogWarning("[Load] Không có file: " + pathToLoad);
            return;
        }

        string json = File.ReadAllText(pathToLoad);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        RoomStorage.rooms = new List<Room>();

        foreach (var path in saveData.paths)
        {
            Room room = new Room();
            room.SetID(path.roomID);
            room.checkpoints = path.points.ConvertAll(p => p.ToVector2());
            room.heights = new List<float>(path.heights);
            // room.wallLines = path.wallLines.ConvertAll(w => new WallLine(w.start, w.end, w.type, w.distanceHeight, w.Height));
            room.wallLines = path.wallLines.ConvertAll(w =>
            {
                var line = new WallLine(w.start, w.end, w.type, w.distanceHeight, w.Height);
                line.isManualConnection = w.isManualConnection; // <--- quan trọng
                return line;
            });

            room.Compass = path.compass.ToVector2();
            room.headingCompass = path.headingCompass;
            RoomStorage.rooms.Add(room);
        }

        Debug.Log("[Load] Loaded " + RoomStorage.rooms.Count + " rooms from: " + fileName);
        SceneManager.LoadScene("FlatExampleScene");
    }

    public static bool DoesNameExist(string baseName)
    {
        string folderPath = Application.persistentDataPath;
        string[] files = Directory.GetFiles(folderPath, "*.json");

        foreach (string path in files)
        {
            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data.paths.Count > 0 && data.paths[0].roomID == baseName)
                    return true;
            }
            catch
            {
            }
        }

        return false;
    }

    public static List<JsonFileInfo> GetAllSavedFileInfos()
    {
        List<JsonFileInfo> infos = new List<JsonFileInfo>();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string path in files)
        {
            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                string fileName = Path.GetFileName(path);
                string name = (data.paths.Count > 0) ? data.paths[0].roomID : fileName;
                string time = data.timestamp;

                infos.Add(new JsonFileInfo
                {
                    fileName = fileName,
                    displayName = name,
                    timestamp = time
                });
            }
            catch
            {
                Debug.LogWarning("Bỏ qua file lỗi: " + path);
            }
        }

        return infos;
    }


    public static bool TryDeleteFile(string fileName)
    {
        try
        {
            string fullFileName = $"{fileName}.json";
            string fullFilePath = Path.Combine(Application.persistentDataPath, fullFileName);

            Debug.Log($"Input file name {fileName}");
            Debug.Log($"Full file name {fullFileName}");
            Debug.Log($"Full file path {fullFilePath}");

            if (!File.Exists(fullFilePath))
            {
                return false;
            }

            //
            Debug.Log("Delete file");
            File.Delete(fullFilePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error Log {e.Message}");
            throw;
        }
    }

    public static bool ChangeFileName(string currentFileName, string newFileName)
    {
        if (string.IsNullOrWhiteSpace(currentFileName) || string.IsNullOrWhiteSpace(newFileName))
        {
            Debug.LogError("[ChangeFileName] File names must not be empty or whitespace.");
            return false;
        }

        try
        {
            currentFileName = EnsureJsonExtension(currentFileName);
            newFileName = EnsureJsonExtension(newFileName);

            string oldFilePath = Path.Combine(Application.persistentDataPath, currentFileName);
            string newFilePath = Path.Combine(Application.persistentDataPath, newFileName);

            if (!File.Exists(oldFilePath))
            {
                Debug.LogError($"[ChangeFileName] Old file path '{oldFilePath}' does not exist.");
                return false;
            }

            if (File.Exists(newFilePath))
            {
                Debug.LogError($"[ChangeFileName] New file path '{newFilePath}' already exists.");
                return false;
            }

            File.Move(oldFilePath, newFilePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ChangeFileName] Unexpected error: {e.Message}");
            return false;
        }
    }


    public static string EnsureJsonExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".json";
        }

        return fileName;
    }
}