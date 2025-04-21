using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class SaveSlotItem : MonoBehaviour
{
    public Image thumbnail;
    public TextMeshProUGUI fileNameText;
    public TextMeshProUGUI timeText;
    public Button loadButton;

    public void Setup(Sprite image, string fileName, string time, UnityAction onClickAction)
    {
        thumbnail.sprite = image;
        fileNameText.text = fileName;
        timeText.text = time;
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(onClickAction);
    }
}
