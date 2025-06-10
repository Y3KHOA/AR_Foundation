using UnityEngine;
using UnityEngine.UI;

public class ActionPanelController : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnEdit;
    public Button btnDelete;

    [Header("Panels")]
    public GameObject panelEdit;
    public GameObject panelDelete;

    void Start()
    {
        // Gán sự kiện click
        btnEdit.onClick.AddListener(OnEditClicked);
        btnDelete.onClick.AddListener(OnDeleteClicked);
        // btnCancelEdit.onClick.AddListener(CloseEditPanel);
        // btnCancelDelete.onClick.AddListener(CloseDeletePanel);

        // Ẩn panel lúc đầu
        panelEdit.SetActive(false);
        panelDelete.SetActive(false);
    }

    void OnEditClicked()
    {
        panelEdit.SetActive(true);
    }

    void OnDeleteClicked()
    {
        panelDelete.SetActive(true);
    }

    // void CloseEditPanel()
    // {
    //     panelEdit.SetActive(false);
    // }

    // void CloseDeletePanel()
    // {
    //     panelDelete.SetActive(false);
    // }
}
