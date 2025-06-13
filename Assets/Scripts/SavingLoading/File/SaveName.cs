using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveName : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField inputField;
    public Button confirmButton;

    [Header("UI Panels")]
    public GameObject panelSave;
    public GameObject panelSuccess;
    public GameObject panelError;

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmSave);
    }

    public void OnConfirmSave()
    {
        string fileName = inputField != null ? inputField.text.Trim() : "";

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("Name not valid.");
            if (panelError != null)
                panelError.SetActive(true);
            return;
        }

        if (SaveLoadManager.DoesNameExist(fileName))
        {
            Debug.LogWarning("Name already exists.");
            if (panelError != null)
                panelError.SetActive(true);
            return;
        }

        SaveLoadManager.Save(fileName);

        if (panelSave != null)
            panelSave.SetActive(false);

        if (panelSuccess != null)
            panelSuccess.SetActive(true);
    }
}
