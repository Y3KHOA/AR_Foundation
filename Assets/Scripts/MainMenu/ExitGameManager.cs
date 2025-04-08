using UnityEngine;
using UnityEngine.UI;

public class ExitGameManager : MonoBehaviour
{
    public Button exitButton; // Gán Button thoát vào đây

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    public void ExitGame()
    {
        // Xóa toàn bộ dữ liệu đã lưu
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // Đảm bảo dữ liệu đã được xóa hoàn toàn

        // Thoát ứng dụng
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Dừng Play Mode trong Editor
        #else
            Application.Quit(); // Thoát ứng dụng khi chạy build
        #endif
    }
}
