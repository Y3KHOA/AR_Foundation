using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image backgroundIcon;
    [SerializeField] private Image backgroundToast;
    [SerializeField] private Image fillTimeAmountImg;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button turnOffBtn;

    public Sprite Icon
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }
    
    public string TitleText
    {
        get => titleText.text;
        set => titleText.text = value;
    }

    public string DescriptionText
    {
        get => descriptionText.text;
        set => descriptionText.text = value;
    }

    [SerializeField] private ToastUIColorPreset preset;

    private void Awake()
    {
        turnOffBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private Tween tween;
    private void OnEnable()
    {
        tween?.Kill();
        tween = DOVirtual.Float(0, 1, 2, value =>
        {
            DoFillAmount(value);
        }).OnComplete(() =>
        {
            gameObject.SetActive(false);
        }).SetEase(Ease.InSine);
    }

    private void OnDisable()
    {
        tween?.Kill();
    }

    public void AssignToastPreset(ToastUIColorPreset preset)
    {
        this.preset = preset;
        Apply();
    }

    public void DoFillAmount(float fillAmount)
    {
        fillTimeAmountImg.fillAmount = fillAmount;
    }

    
    [ContextMenu("Apply Preset")]
    private void Apply()
    {
        if (preset == null) return;

        icon.sprite = preset.icon;
        backgroundIcon.color = preset.backgroundIconColor;
        backgroundToast.color = preset.backgrondToastColor;
    }
}