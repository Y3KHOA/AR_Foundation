using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class SceneSwitcherEditor : EditorWindow
{
    private List<string> scenesList = new List<string>();
    private Vector2 scrollPosition;

    // Add menu item named "Scenes" to the menu bar
    [MenuItem("Scenes/Open Scene Browser", false, 0)]
    public static void ShowWindow()
    {
        // Show existing window or create one
        GetWindow<SceneSwitcherEditor>("Scene Browser");
    }
    private SceneSwitcherConfig config;

    private void OnEnable()
    {
        // Refresh scene list when window is opened
        config = SceneSwitcherConfig.GetOrCreateConfig();
        RefreshSceneList();
    }

    private void RefreshSceneList()
    {
        scenesList.Clear();

        // Find all scene files in the project
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            path = path.Replace("\\", "/");

            if (!string.IsNullOrWhiteSpace(config.sceneFolderPath))
            {
                string normalizedValidPath = config.sceneFolderPath.Replace("\\", "/").Trim();

                if (!normalizedValidPath.EndsWith("/")) 
                    normalizedValidPath += "/";

                if (!path.StartsWith(normalizedValidPath))
                    continue;
            }
            
            scenesList.Add(path);
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            fixedHeight = 30
        };
        if (GUILayout.Button(new GUIContent(" Open Config File"), buttonStyle))
        {
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh Scene List", buttonStyle))
        {
            RefreshSceneList();
        }

        EditorGUILayout.EndHorizontal();
        
        // GUILayout.Label("Available Scenes", EditorStyles.boldLabel);
        // if (GUILayout.Button("Open Config File in Project"))
        // {
        //     Selection.activeObject = config;
        //     EditorGUIUtility.PingObject(config);
        // }
        // if (GUILayout.Button("Refresh Scene List"))
        // {
        //     RefreshSceneList();
        // }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (string scenePath in scenesList)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Get scene name from path
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            
            if (GUILayout.Button(sceneName, GUILayout.ExpandWidth(true)))
            {
                // Check if this is already the active scene
                string currentScenePath = EditorSceneManager.GetActiveScene().path;
                if (currentScenePath == scenePath)
                {
                    // Already in this scene, no need to load
                    EditorUtility.DisplayDialog("Information", 
                        "This scene is already open.", "OK");
                    return;
                }
                
                // Check if current scene has unsaved changes
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    // Ask user if they want to save changes with a popup dialog
                    int option = EditorUtility.DisplayDialogComplex("Unsaved Changes", 
                        "The current scene has unsaved changes. Do you want to save them before loading the new scene?", 
                        "Save", "Don't Save", "Cancel");
                        
                    switch (option)
                    {
                        // Save
                        case 0:
                            EditorSceneManager.SaveOpenScenes();
                            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                            break;
                        // Don't Save
                        case 1:
                            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                            break;
                        // Cancel
                        case 2:
                            // Do nothing, stay in current scene
                            return;
                    }
                }
                else
                {
                    // No changes to save, just open the selected scene
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}

// This class adds the "Scenes" menu to the main menu bar
[InitializeOnLoad]
public class ScenesMenuInitializer
{
    static ScenesMenuInitializer()
    {
        // This gets called when Unity Editor starts or when scripts are recompiled
        // The menu item is already defined by the SceneSwitcherEditor class
    }
}