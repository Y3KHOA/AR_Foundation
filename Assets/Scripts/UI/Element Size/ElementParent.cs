using UnityEngine;
using UnityEngine.UI;

public class ElementParent : MonoBehaviour
{
    [Header("Rect Transform")]
    [SerializeField] private RectTransform itemBar;
    [SerializeField] private RectTransform actionView;
    [SerializeField] private RectTransform canvas;
    [Header("References")]
    [SerializeField] private CompassMenu compassMenu;

    [SerializeField] private Button toggleButton;
    public bool isActiveBar = false;

    private void Start()
    {
        isActiveBar = false;
        toggleButton.onClick.AddListener(() =>
        {
            isActiveBar = !isActiveBar;
        });
        itemBar.gameObject.SetActive(false);

        CalculatorRatio();
    }

    private void CalculatorRatio()
    {
        var leftOffset = -itemBar.offsetMin.x;
        var fullWidth = actionView.rect.width;

        var activeWidth = fullWidth - leftOffset;
        ratio = activeWidth / fullWidth;
    }

    private float ratio;
    private bool previousIsActive;

    private void Update()
    {
        // resize by setting anchor and reset offset
        if (previousIsActive != isActiveBar)
        {
            CalculatorRatio();

            actionView.anchorMax = !isActiveBar ? new Vector2(1, 1) : new Vector2(ratio, 1);
            actionView.anchorMin = !isActiveBar ? new Vector2(0, 0) : new Vector2(0, 0);
            actionView.offsetMin = Vector2.zero;
            actionView.offsetMax = Vector2.zero;

            previousIsActive = isActiveBar;

            compassMenu.Refresh();

            itemBar.gameObject.SetActive(isActiveBar);
        }
    }
}