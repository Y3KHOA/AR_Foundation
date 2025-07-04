using UnityEngine;
using UnityEditor;
using System;

public class FindScriptInProject : EditorWindow
{
    private string scriptName = "";

    [MenuItem("Tools/Find Script Usage In Project")]
    public static void ShowWindow()
    {
        GetWindow<FindScriptInProject>("Find Script In Project");
    }

    void OnGUI()
    {
        GUILayout.Label("--> Find Script In Project (Prefab / Assets)", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField("Script Name", scriptName);

        if (GUILayout.Button("Search"))
        {
            FindScriptUsageInProject();
        }
    }

    void FindScriptUsageInProject()
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            Debug.LogWarning("<!!!> Please enter a script name!");
            return;
        }

        Type scriptType = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            scriptType = assembly.GetType(scriptName);
            if (scriptType != null) break;

            scriptType = Array.Find(assembly.GetTypes(), t => t.Name == scriptName);
            if (scriptType != null) break;
        }

        if (scriptType == null)
        {
            Debug.LogError($"<X> Could not find type '{scriptName}'. Check spelling / namespace.");
            return;
        }

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var component = prefab.GetComponentInChildren(scriptType, true);
            if (component != null)
            {
                Debug.Log($"--> Found in Prefab: {path}", prefab);
                count++;
            }
        }

        Debug.Log($"--> Search completed. Found {count} prefab(s) with script '{scriptName}'.");
    }
}
