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
            GameObject item = Instantiate(dataItemPrefab, contentParent);

            string nameOnly = Path.GetFileNameWithoutExtension(file.fileName);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var txt in texts)
            {
                if (txt.name.Contains("Name"))
                    txt.text = nameOnly;
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
}
