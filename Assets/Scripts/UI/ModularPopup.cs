using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModularPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI yesBtnText;
    [SerializeField] private TextMeshProUGUI noBtnText;

    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;

    public Action ClickYesEvent;
    public Action ClickNoEvent;
    public Action EventWhenClickButtons;

    public bool autoClearWhenClick = false;

    public string Header
    {
        get => headerText.text;
        set => headerText.text = value;
    }

    public string YesText
    {
        get => yesBtnText.text;
        set => yesBtnText.text = value;
    }

    public string NoText
    {
        get => noBtnText.text;
        set => noBtnText.text = value;
    }

    private void Awake()
    {
        yesBtn.onClick.AddListener(OnYesClicked);
        noBtn.onClick.AddListener(OnNoClicked);
    }

    private void OnYesClicked()
    {
        ClickYesEvent?.Invoke();
        EventWhenClickButtons?.Invoke();
        TryToClear();
    }

    private void OnNoClicked()
    {
        ClickNoEvent?.Invoke();
        EventWhenClickButtons?.Invoke();
        TryToClear();
    }

    private void TryToClear()
    {
        if (autoClearWhenClick)
        {
            Destroy(gameObject);
        }
    }

    public void ResetAnchorOffsetAndScale()
    {
        var popupRect = GetComponent<RectTransform>();
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;
        popupRect.transform.localScale = Vector3.one;


    }
}