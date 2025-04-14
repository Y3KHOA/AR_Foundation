using UnityEditor;
using UnityEditor.SceneManagement;

public class QuickSceneLoader
{
    [MenuItem("Tools/Load Test Scene")]
    static void LoadTestScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
    }
}
