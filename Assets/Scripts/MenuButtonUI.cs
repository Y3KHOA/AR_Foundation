using System;
using TMPro;
using UnityEngine;

public class MenuButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private ButtonTextData buttonTextData;

    private void LoadData()
    {
        if (buttonTextData == null)
        {
            Debug.LogError("Kiểm tra lại xem đã có button text data chưa",gameObject);
            return;
        }
        
        titleText.text = buttonTextData.titleTextData.data;
        descriptionText.text = buttonTextData.descriptionTextData.data;
    }

    private void PreviewData()
    {
        LoadData();
    }
    
}
[CreateAssetMenu(fileName = "Button_Default",menuName = "SO/Button Text Data")]
public class ButtonTextData : ScriptableObject
{

    public TextData titleTextData;
    public TextData descriptionTextData;
    
    [Serializable]
    public struct TextData
    {
        public string languageTag;
        public string data;
    }
}
