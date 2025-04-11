using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ButtonOptionsModel : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnClose;
    public Button btnOpen;

    [Header("Menu Objects")]
    public GameObject ObClose;
    public GameObject ObOpen;

    [Header("UI Text")]
    public BtnController btnController;
    public TextMeshProUGUI heightText;

    void Start()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(CloseOptions);

        if (btnOpen != null)
            btnOpen.onClick.AddListener(OpenOptions);

        ObClose?.SetActive(false);
        ObOpen?.SetActive(true);
    }
    void Update()
    {
        UpdateHeightDisplay();
    }

    void OpenOptions()
    {
        Debug.Log("Open Menu");
        ObOpen?.SetActive(false);
        ObClose?.SetActive(true);
    }

    void CloseOptions()
    {
        Debug.Log("Close Menu");
        ObClose?.SetActive(false);
        ObOpen?.SetActive(true);
    }

    void UpdateHeightDisplay()
    {
        heightText.text = $"Height: {btnController.heightValue:F2} m";
    }
}
