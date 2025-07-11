using System;
using TMPro;
using UnityEngine;

public class MenuButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [ContextMenu("Test")]
    private void Test()
    {
        var items = FindObjectsByType<MenuButtonUI>();
        foreach (var item in items)
        {
            item.titleText = item.titleText.text.ToLower();
        }
    }
    
}
