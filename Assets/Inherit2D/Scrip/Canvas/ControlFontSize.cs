using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ControlFontSize : MonoBehaviour
{
    private List<TextMeshProUGUI> textsList = new List<TextMeshProUGUI>();

    private void Start()
    {
        textsList = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        //ControlSize();
    }

    public void ControlSize()
    {
        float minFont = float.MaxValue;
        for (int i = 0; i < textsList.Count; i++)
        {
            if (textsList[i].fontSize < minFont)
                minFont = textsList[i].fontSize;
        }

        for (int i = 0;i < textsList.Count; i++)
        {
            textsList[i].fontSizeMax = minFont;
        }
    }    
}
