using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoredDrawManager : MonoBehaviour
{
    public static StoredDrawManager Instance;
    [SerializeField] private TMP_InputField fileNameInputField;
    [SerializeField] private Transform changeNamePanel;
    [SerializeField] private Transform deletePanel;

    [SerializeField] private Button changeNameConfirmBtn;
    [SerializeField] private Button deleteButton;

    [Header("Test delete file ")]
    [SerializeField] private LoadFile loadFile;

    private string currentFileName;

    private void Awake()
    {
        Instance = this;
        changeNameConfirmBtn.onClick.AddListener(OnConfirmChangeFileName);
        deleteButton.onClick.AddListener(OnConfirmDeleteFile);
    }

    private void OnDestroy()
    {
        changeNameConfirmBtn.onClick.RemoveListener(OnConfirmChangeFileName);
        deleteButton.onClick.RemoveListener(OnConfirmDeleteFile);
    }

    public void ShowChangeNamePanel(StoredDrawUI storedDrawUI)
    {
        currentFileName = storedDrawUI.fileName;

        fileNameInputField.text = storedDrawUI.fileName;
        changeNamePanel.gameObject.SetActive(true);
        fileNameInputField.Select();
    }

    private void OnConfirmChangeFileName()
    {
        string newFileName = fileNameInputField.text;
        
        if (string.IsNullOrWhiteSpace(newFileName)) return;
        if (!SaveLoadManager.ChangeFileName(currentFileName, newFileName)) return;
        
        loadFile.LoadAllSavedFiles();
        changeNamePanel.gameObject.SetActive(false);
    }

    private void OnConfirmDeleteFile()
    {
        if (SaveLoadManager.TryDeleteFile(currentFileName))
        {
            loadFile.LoadAllSavedFiles();
            deletePanel.gameObject.SetActive(false);
        }
    }

    public void ShowDeletePanel(StoredDrawUI storedDrawUI)
    {
        currentFileName = storedDrawUI.fileName;
        deletePanel.gameObject.SetActive(true);
    }

// #if UNITY_EDITOR
//     [Header("Test change file Name")]
//     [SerializeField] private string testChangeFileName;
//     [SerializeField] private string testNewFileName;
//     [SerializeField] private string fileNameTest;
//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.B))
//         {
//             if (SaveLoadManager.TryDeleteFile(fileNameTest))
//             {
//                 loadFile.LoadAllSavedFiles();
//             }
//         }
//
//         if (Input.GetKeyDown(KeyCode.K))
//         {
//             SaveLoadManager.ChangeFileName(testChangeFileName,testNewFileName);
//             loadFile.LoadAllSavedFiles();
//         }
//     }
//     #endif
}