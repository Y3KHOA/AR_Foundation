using UnityEngine;
using UnityEditor;
using System.IO;
    
[CreateAssetMenu(fileName = "SceneSwitcherConfig", menuName = "Tools/Scene Switcher Config")]
public class SceneSwitcherConfig : ScriptableObject
{
    public string sceneFolderPath = "Assets/Scenes";

#if UNITY_EDITOR
    private const string ConfigAssetPath = "Assets/Editor/SceneSwitcherConfig.asset";

    public static SceneSwitcherConfig GetOrCreateConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<SceneSwitcherConfig>(ConfigAssetPath);

        if (config == null)
        {
            // Ensure folder exists
            string dir = Path.GetDirectoryName(ConfigAssetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            config = ScriptableObject.CreateInstance<SceneSwitcherConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneSwitcher] Created new config at: {ConfigAssetPath}");
        }

        return config;
    }
#endif
}