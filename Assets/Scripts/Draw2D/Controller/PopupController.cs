using UnityEngine;
using UnityEngine.UI;

public static class PopupController
{
    public static void Show(string message, System.Action onYes, System.Action onNo)
    {
        // === Tạo Canvas popup ===
        GameObject popupGO = new GameObject("Popup");
        popupGO.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        popupGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        popupGO.AddComponent<GraphicRaycaster>();

        // === Background ===
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(popupGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // === Panel ===
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(bgGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = Color.white;
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 200);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        // === Text ===
        GameObject textGO = new GameObject("Message");
        textGO.transform.SetParent(panelGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // === OK ===
        GameObject okGO = CreateButton("OK", panelGO.transform, new Vector2(-60, -60));
        okGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            onYes?.Invoke();
            GameObject.Destroy(popupGO);
        });

        // === Cancel ===
        GameObject cancelGO = CreateButton("Cancel", panelGO.transform, new Vector2(60, -60));
        cancelGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            onNo?.Invoke();
            GameObject.Destroy(popupGO);
        });
    }

    private static GameObject CreateButton(string label, Transform parent, Vector2 anchoredPos)
    {
        GameObject btnGO = new GameObject(label + "Button");
        btnGO.transform.SetParent(parent, false);

        Button btn = btnGO.AddComponent<Button>();
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 40);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;

        RectTransform txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btnGO;
    }
}
