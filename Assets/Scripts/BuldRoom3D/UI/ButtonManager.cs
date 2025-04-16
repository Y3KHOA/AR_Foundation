using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnFloor;
    public Button btn3D;
    public Button btnInfo;

    [Header("Buttons Child")]
    public Button btnEdit;

    [Header("Panels")]
    public GameObject panelFloor;
    public GameObject panel3D;
    public GameObject panelInfo;
    public GameObject previewTexture;

    private void Start()
    {
        StartCoroutine(SetupUI());

        // Gán sự kiện click sau khi setup UI
        btnFloor.onClick.AddListener(() => OnButtonFloor(panelFloor));
        btnEdit.onClick.AddListener(() => OnButtonEdit());
        btn3D.onClick.AddListener(() => OnButton3D(panel3D));
        btnInfo.onClick.AddListener(() => OnButtonInfo(panelInfo));
    }
    IEnumerator SetupUI()
    {
        yield return null; // đợi 1 frame

        if (ButtonOk.IsOkButtonShown)
        {
            previewTexture.SetActive(true);
            panelFloor.SetActive(false);
            panel3D.SetActive(true);
            panelInfo.SetActive(false);
            btnEdit.gameObject.SetActive(false);
        }
        else
        {
            HideAllPanels();
        }
    }


    private void OnButtonFloor(GameObject selectedPanel)
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(true);
        panel3D.SetActive(false);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(true);
    }
    private void OnButton3D(GameObject selectedPanel)
    {
        previewTexture.SetActive(true);
        panelFloor.SetActive(false);
        panel3D.SetActive(true);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(false);
    }
    private void OnButtonInfo(GameObject selectedPanel)
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(false);
        panel3D.SetActive(false);
        panelInfo.SetActive(true);
        btnEdit.gameObject.SetActive(false);
    }

    void OnButtonEdit()
    {
        Debug.Log("Button Edit clicked!");
        TransData.Instance.TransferData();
        SceneManager.LoadScene("DraftingScene");
    }

    private void HideAllPanels()
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(false);
        panel3D.SetActive(false);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(false);
    }
}
