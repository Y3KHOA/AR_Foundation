using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadFile : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform contentParent;        // Gán DataLoader
    public GameObject dataItemPrefab;      // Prefab cho mỗi file hiển thị

    void OnEnable()
    {
        LoadAllSavedFiles();
    }

    public void LoadAllSavedFiles()
    {
        // Xoá các mục cũ trong danh sách
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<JsonFileInfo> files = SaveLoadManager.GetAllSavedFileInfos();

        foreach (var file in files)
        {
            string displayName = Path.GetFileNameWithoutExtension(file.fileName);
            if (dataItemPrefab.TryGetComponent(out StoredDrawUI storedDrawUIPrefab))
            {
                StoredDrawUI storedUI = Instantiate(storedDrawUIPrefab, contentParent);
                string fileName = file.fileName;
                
                Setup(storedUI, displayName, fileName);

                continue;
            }
            GameObject item = Instantiate(dataItemPrefab, contentParent);


            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var txt in texts)
            {
                if (txt.name.Contains("Name"))
                    txt.text = displayName;
                else if (txt.name.Contains("Date"))
                    txt.text = "Date: " + file.timestamp;
            }

            // Nếu prefab có Button, cho phép click để Load
            Button btn = item.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string fname = file.fileName; // cần copy vào biến để tránh closure bug
                btn.onClick.AddListener(() =>
                {
                    SaveLoadManager.Load(fname);
                    transform.gameObject.SetActive(false);
                });
                
            }
        }
    }

    private static void Setup(StoredDrawUI storedUI, string displayName, string fileName)
    {
        storedUI.nameText.text = displayName;
        storedUI.fileName = displayName;
        storedUI.loadBtn.onClick.AddListener(() =>
        {
            SaveLoadManager.Load(fileName);
        });
        storedUI.editNameBtn.onClick.AddListener(() =>
        {
            StoredDrawManager.Instance.ShowChangeNamePanel(storedUI);
        });
        storedUI.deleteFileBtn.onClick.AddListener(() =>
        {
            StoredDrawManager.Instance.ShowDeletePanel(storedUI);
        });
    }
}
