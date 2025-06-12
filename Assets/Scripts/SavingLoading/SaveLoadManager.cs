using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadManager
{
    // private static string savePath = Path.Combine(Application.persistentDataPath, "Data/savedData.json");
    // private static string savePath = Path.Combine("/storage/emulated/0/Download", "XHeroScan/Data/Drawing_All_Test1.json");
    
    // public static void SaveJsonToDownloads(string fileName, string jsonContent)
    // {
    // #if UNITY_ANDROID && !UNITY_EDITOR
    //     try
    //     {
    //         using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    //         {
    //             AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    //             AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

    //             AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");
    //             AndroidJavaObject contentValues = new AndroidJavaObject("android.content.ContentValues");

    //             contentValues.Call("put", "title", fileName);
    //             contentValues.Call("put", "_display_name", fileName);
    //             contentValues.Call("put", "mime_type", "application/json");
    //             contentValues.Call("put", "relative_path", "Download/XHeroScan/Data");

    //             long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    //             contentValues.Call("put", "date_added", new AndroidJavaObject("java.lang.Long", currentTime / 1000));
    //             contentValues.Call("put", "date_modified", new AndroidJavaObject("java.lang.Long", currentTime / 1000));

    //             AndroidJavaObject externalUri = mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");

    //             AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", externalUri, contentValues);
    //             if (uri == null)
    //             {
    //                 Debug.LogError("MediaStore insert returned null.");
    //                 return;
    //             }

    //             AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
    //             if (outputStream == null)
    //             {
    //                 Debug.LogError("Cannot open output stream.");
    //                 return;
    //             }

    //             byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
    //             outputStream.Call("write", bytes);
    //             outputStream.Call("flush");
    //             outputStream.Call("close");

    //             Debug.Log("JSON saved using MediaStore.");
    //         }
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError("Failed to save JSON using MediaStore: " + ex.Message);
    //     }
    // #else
    //     // Editor / fallback
    //     string fallbackPath = Path.Combine(Application.persistentDataPath, fileName);
    //     File.WriteAllText(fallbackPath, jsonContent);
    //     Debug.Log("Saved locally (Editor): " + fallbackPath);
    // #endif
    // }

    public static string SaveJsonToDownloads(string fileName, string jsonContent)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

                AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");
                AndroidJavaObject contentValues = new AndroidJavaObject("android.content.ContentValues");

                contentValues.Call("put", "title", fileName);
                contentValues.Call("put", "_display_name", fileName);
                contentValues.Call("put", "mime_type", "application/json");
                contentValues.Call("put", "relative_path", "Download/XHeroScan/Data");

                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                contentValues.Call("put", "date_added", new AndroidJavaObject("java.lang.Long", currentTime / 1000));
                contentValues.Call("put", "date_modified", new AndroidJavaObject("java.lang.Long", currentTime / 1000));

                AndroidJavaObject externalUri = mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");

                AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", externalUri, contentValues);
                if (uri == null)
                {
                    Debug.LogError("MediaStore insert returned null.");
                    return "Insert failed";
                }

                AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
                if (outputStream == null)
                {
                    Debug.LogError("Cannot open output stream.");
                    return "OutputStream failed";
                }

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
                outputStream.Call("write", bytes);
                outputStream.Call("flush");
                outputStream.Call("close");

                string uriString = uri.Call<string>("toString");
                Debug.Log("JSON saved at URI: " + uriString);
                return uriString;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception saving JSON: " + ex.Message);
            return "Exception: " + ex.Message;
        }
    #else
        string fallbackPath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(fallbackPath, jsonContent);
        Debug.Log("Saved locally (Editor): " + fallbackPath);
        return fallbackPath;
    #endif
    }

    public static void Save()
    {
        SaveData saveData = new SaveData
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        };

        foreach (Room room in RoomStorage.rooms)
        {
            SavedPath path = new SavedPath();
            path.roomID = room.ID;  // Gán ID vào SavedPath

            // Checkpoints
            path.points = new List<Vector2Serializable>();
            foreach (var point in room.checkpoints)
            {
                path.points.Add(new Vector2Serializable(point));
            }

            // Heights
            path.heights = new List<float>(room.heights);

            // WallLines
            path.wallLines = new List<SavedWallLine>();
            foreach (var wall in room.wallLines)
            {
                path.wallLines.Add(new SavedWallLine
                {
                    start = wall.start,
                    end = wall.end,
                    type = wall.type,
                    distanceHeight = wall.distanceHeight,
                    Height = wall.Height
                });
            }

            // Compass
            path.compass = new Vector2Serializable(room.Compass);
            path.headingCompass = room.headingCompass;

            saveData.paths.Add(path);
        }

        // string json = JsonUtility.ToJson(saveData, true);

        // string directory = Path.GetDirectoryName(savePath);
        // if (!Directory.Exists(directory))
        //     Directory.CreateDirectory(directory);

        // File.WriteAllText(savePath, json);

        string json = JsonUtility.ToJson(saveData, true);
        string savedPath = SaveJsonToDownloads("Drawing_All_Test1.json", json);

        Debug.Log("[Save] Ok to: " + savedPath);
    }


    public static void Load()
    {
        string fallbackPath = Path.Combine(Application.persistentDataPath, "Drawing_All_Test1.json");

    #if UNITY_ANDROID && !UNITY_EDITOR
        string pathToLoad = fallbackPath;
    #else
        string pathToLoad = fallbackPath;
    #endif

        if (!File.Exists(pathToLoad))
        {
            Debug.LogWarning("[Load] Error: File not found at " + pathToLoad);
            return;
        }

        string json = File.ReadAllText(pathToLoad);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        RoomStorage.rooms = new List<Room>();

        foreach (var path in saveData.paths)
        {
            Room room = new Room();
            room.SetID(path.roomID);

            foreach (var v in path.points)
                room.checkpoints.Add(v.ToVector2());

            room.heights = new List<float>(path.heights);

            room.wallLines = new List<WallLine>();
            foreach (var wall in path.wallLines)
            {
                WallLine wallLine = new WallLine(wall.start, wall.end, wall.type, wall.distanceHeight, wall.Height);
                room.wallLines.Add(wallLine);
            }

            room.Compass = path.compass.ToVector2();
            room.headingCompass = path.headingCompass;

            RoomStorage.rooms.Add(room);
        }

        Debug.Log("[Load] Loaded " + RoomStorage.rooms.Count + " rooms.");
        SceneManager.LoadScene("FlatExampleScene");
    }
}
