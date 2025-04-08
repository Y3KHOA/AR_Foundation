using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClosePanelOnClick : MonoBehaviour
{
    public GameObject warningPanel;

    void Start()
    {
        // Gán sự kiện click cho vùng cho phép click để đóng
        if (warningPanel != null)
        {
            AddEventTrigger(warningPanel, EventTriggerType.PointerClick, (eventData) => ClosePanel());
        }
    }
        // Hàm ẩn panel cảnh báo
    public void ClosePanel()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    // Hàm thêm sự kiện vào EventTrigger của vùng cho phép click
    private void AddEventTrigger(GameObject obj, EventTriggerType type, System.Action<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(action));
        trigger.triggers.Add(entry);
    }
}
