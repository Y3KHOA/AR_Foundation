using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuButtonUI))]
public class MenuButtonUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Preview Data"))
        {
            var targetScript = target as MenuButtonUI;
            MethodInfo method = typeof(MenuButtonUI).GetMethod("Test", BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (method != null)
            {
                method.Invoke(targetScript, null);
            }
            else
            {
                Debug.LogWarning("Method not found!");
            }
        }
    }
}