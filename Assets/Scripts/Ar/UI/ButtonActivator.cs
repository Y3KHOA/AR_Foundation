using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ButtonActivator : MonoBehaviour
{
    public Button targetButton; // Button cần bật
    public BtnController btnController; // Script chứa dữ liệu tọa độ

    void Start()
    {
        if (targetButton != null)
            targetButton.onClick.AddListener(OnTargetButtonClicked);
    }

    void Update()
    {
        if (btnController.Flag == 1 && !targetButton.gameObject.activeSelf)
        {
            targetButton.gameObject.SetActive(true);
            Debug.Log("Button enabled");
        }
    }

    void OnTargetButtonClicked()
    {
        Debug.Log("Button clicked - To DraftingScene");
        TransData.Instance.TransferData();
        SceneManager.LoadScene("DraftingScene");
    }
}
