using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BtnChoose : MonoBehaviour
{
    public Button btnDraw;
    public Button btnByCam;
    public GameObject ObjectByCam; // GameObject sẽ hiện khi nhấn ByCam

    void Start()
    {
        // Gán sự kiện khi nhấn nút
        if (btnDraw != null)
            btnDraw.onClick.AddListener(OnDrawClicked);

        if (btnByCam != null)
            btnByCam.onClick.AddListener(OnByCamClicked);

        // Ẩn ObjectByCam lúc đầu
        if (ObjectByCam != null)
            ObjectByCam.SetActive(false);
    }

    void OnDrawClicked()
    {
        Debug.Log("Nhấn Draw - Chuyển scene DraftingScene");
        SceneManager.LoadScene("SampleScene"); // Chuyển scene
    }

    void OnByCamClicked()
    {
        Debug.Log("Nhấn ByCam - Hiện ObjectByCam và ẩn 2 button");

        if (ObjectByCam != null)
            ObjectByCam.SetActive(true); // Hiện ObjectByCam

        if (btnDraw != null)
        {
            btnDraw.gameObject.SetActive(false); // Ẩn nút Draw
        }

        if (btnByCam != null)
        {
            btnByCam.gameObject.SetActive(false); // Ẩn nút ByCam
        }
    }
}
