using UnityEngine;
using UnityEngine.UI;

public class PanelBlocker : MonoBehaviour
{
    [Header("Panel bạn muốn theo dõi")]
    public GameObject panel;

    [Header("Button sẽ bị khóa khi Panel bật")]
    public Button targetButton;

    [Header("Button dùng để reset trạng thái")]
    public Button resetButton;

    private bool lastPanelState = false;
    private bool isReset = false;

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() =>
            {
                Debug.Log("Reset button clicked!");
                isReset = true;
                targetButton.interactable = true; // Mở lại button ngay lập tức
            });
        }
    }

    void Update()
    {
        if (panel != null && targetButton != null)
        {
            bool currentPanelState = panel.activeSelf;

            if (currentPanelState != lastPanelState)
            {
                if (!isReset)
                {
                    // Chỉ chạy logic bình thường nếu chưa bấm reset
                    targetButton.interactable = !currentPanelState;
                }

                lastPanelState = currentPanelState;
            }
        }
    }
}
