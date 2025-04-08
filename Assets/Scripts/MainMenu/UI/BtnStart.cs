using UnityEngine;
using UnityEngine.UI;

public class BtnStart : MonoBehaviour
{
    public GameObject targetObject; // GameObject sẽ được bật khi nhấn nút
    public Button button; // Kéo button vào Inspector nếu cần

    void Start()
    {
        if (button == null) // Nếu chưa được kéo vào, thử tìm trong GameObject này
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            Debug.LogError("Button component not found! Hãy kéo button vào Inspector hoặc đặt script trên Button.");
            return;
        }

        button.onClick.AddListener(OnButtonClick);

        if (targetObject != null)
        {
            targetObject.SetActive(false); // Ban đầu ẩn targetObject
        }
    }

    void OnButtonClick()
    {
        button.gameObject.SetActive(false); // Ẩn chính button này
        if (targetObject != null)
        {
            targetObject.SetActive(true); // Hiện targetObject
        }
    }
}
