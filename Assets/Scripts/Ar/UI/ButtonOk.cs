using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ButtonOk : MonoBehaviour
{
    public BtnController btnController;      // Gán BtnController từ Inspector
    public GameObject okButton;              // Button OK (GameObject chứa Button)
    private static bool isOkButtonShown = false;    // Đảm bảo chỉ hiện một lần
    public static bool IsOkButtonShown { set { isOkButtonShown = value; } get { return isOkButtonShown; } }

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
        TransData.Instance.TransferData();
        SceneManager.LoadScene("FlatExampleScene");
    }
}
