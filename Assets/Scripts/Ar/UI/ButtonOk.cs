using UnityEngine;
using UnityEngine.UI;

public class ButtonOk : MonoBehaviour
{
    public BtnController btnController;      // Gán BtnController từ Inspector
    public GameObject okButton;              // Button OK (GameObject chứa Button)
    private bool isOkButtonShown = false;    // Đảm bảo chỉ hiện một lần

    void Start()
    {
        if (okButton != null)
        {
            okButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowOkButton();          // Mở menu
            });
        }

        okButton.SetActive(false); // Ẩn OK Button ban đầu
    }
    void Update()
    {
        if (btnController.Flag == 1)
        {
            okButton.SetActive(true); // Hiện OK Button nếu flag = 1
        }
    }

    void ShowOkButton()
    {
        isOkButtonShown = true;
        Debug.Log("[ButtonOk] OK Button hien thi do flag=1");
    }
}
