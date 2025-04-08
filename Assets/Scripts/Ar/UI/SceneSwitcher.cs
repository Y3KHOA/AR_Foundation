using UnityEngine;
using UnityEngine.SceneManagement;

/*
    Trong scene AR, tạo một nút UI "Chuyển sang 2D".
    thêm nút button khi nào điểm đầu điểm cuối chạm nhau.
    thì hiện button hoàn thành và chuyển sang 2D.
*/
public class SceneSwitcher : MonoBehaviour
{
    public void SwitchTo2DScene()
    {
        SceneManager.LoadScene("BlueprintScene"); // Tên Scene 2D
    }
}
