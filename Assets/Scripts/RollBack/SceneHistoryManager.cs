using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneHistoryManager : MonoBehaviour
{
    private static SceneHistoryManager instance;
    private static Stack<string> sceneHistory = new Stack<string>(); // Lịch sử scene
    private static Stack<object> undoStack = new Stack<object>(); // Lưu trạng thái để undo

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded; // Lắng nghe sự kiện load scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Lưu scene vào stack khi chuyển scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Chỉ lưu nếu scene khác scene hiện tại để tránh lưu trùng
        if (sceneHistory.Count == 0 || sceneHistory.Peek() != scene.name)
        {
            sceneHistory.Push(scene.name);
            undoStack.Clear(); // Reset undoStack khi chuyển scene
        }

        Debug.Log($"[SceneHistory] Scene hiện tại: {scene.name}");
        Debug.Log($"[SceneHistory] Stack: {string.Join(" -> ", sceneHistory)}");
    }

    // Chuyển sang scene mới
    public static void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // Quay lại scene trước
    public static void LoadPreviousScene()
    {
        if (sceneHistory.Count > 1)
        {
            sceneHistory.Pop();
            string previousScene = sceneHistory.Peek();
            SceneManager.LoadScene(previousScene);
            Debug.Log($"[SceneHistory] Quay lại scene: {previousScene}");
        }
        else
        {
            Debug.LogWarning("[SceneHistory] Không có scene trước!");
        }
    }

    // Lưu trạng thái của scene hiện tại để Undo
    public static void SaveState(object state)
    {
        undoStack.Push(state);
        Debug.Log($"[Undo] Đã lưu trạng thái, stack: {undoStack.Count}");
    }

    // Undo thao tác cuối cùng
    public static void Undo()
    {
        if (undoStack.Count > 0)
        {
            object lastState = undoStack.Pop();
            SceneStateManager.LoadState(lastState);
            Debug.Log($"[Undo] Khôi phục trạng thái, stack còn: {undoStack.Count}");
        }
        else if (sceneHistory.Count > 1)
        {
            Debug.LogWarning("[Undo] Không còn thao tác để hoàn tác, quay lại scene trước!");
            LoadPreviousScene();
        }
        else
        {
            Debug.LogWarning("[Undo] Không có thao tác nào để hoàn tác và không có scene trước!");
        }
    }
}
