using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class FindScriptInScene : EditorWindow
{
    private string scriptName = ""; // Để trống hoặc đặt mặc định

    [MenuItem("Tools/Find Script Usage (Window)")]
    public static void ShowWindow()
    {
        GetWindow<FindScriptInScene>("Find Script In Scene");
    }

    void OnGUI()
    {
        GUILayout.Label("--> Find Script In Scene", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField("Script Name", scriptName);

        if (GUILayout.Button("Search"))
        {
            FindScriptUsage();
        }
    }

    void FindScriptUsage()
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            Debug.LogWarning("<!!!> Please enter a script name!");
            return;
        }

        // Tìm Type trong toàn bộ Assemblies
        Type scriptType = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            // Cách 1: Tìm theo full name
            scriptType = assembly.GetType(scriptName);
            if (scriptType != null) break;

            // Cách 2: Tìm theo tên class nếu không có namespace
            scriptType = assembly.GetTypes().FirstOrDefault(t => t.Name == scriptName);
            if (scriptType != null) break;
        }

        if (scriptType == null)
        {
            Debug.LogError($"<X> Could not find type '{scriptName}'. Check namespace and spelling.");
            return;
        }

        // Tìm các object có chứa script này trong Scene
        var foundObjects = FindObjectsByType(scriptType, FindObjectsSortMode.None);

        if (foundObjects.Length == 0)
        {
            Debug.Log($"<X> No GameObject found with script '{scriptName}'");
        }
        else
        {
            Debug.Log($"--> Found {foundObjects.Length} GameObject(s) with script '{scriptName}':");
            foreach (var obj in foundObjects)
            {
                if (obj is Component component)
                {
                    Debug.Log($"--> {component.gameObject.name}", component.gameObject);
                }
                else if (obj is GameObject go)
                {
                    Debug.Log($"--> {go.name}", go);
                }
            }
        }
    }
}
