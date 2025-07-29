using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button backButton;
    private static Canvas canvas;


    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                if (isShow) return;
                isShow = true;
                Debug.Log("Button Back Clicked!");
                // ShowUnsavedDataPopup();
                ShowPopupPrefab();
            });
        }
        else
        {
            Debug.LogError("BackButton: Chưa gán Button!");
        }
    }

    private bool isShow = false;

    private void ShowPopupPrefab()
    {
        var popup = Instantiate(ModularPopup.Prefab);
        popup.AutoFindCanvasAndSetup();
        popup.transform.rotation = Quaternion.identity;
        popup.transform.localEulerAngles = Vector3.zero;
        popup.Header = "Dữ liệu của bạn chưa được lưu!\nNếu thoát ra sẽ mất dữ liệu!";
        popup.ClickYesEvent = OnClickYes;
        popup.EventWhenClickButtons = () =>
        {
            // BackgroundUI.Instance.Hide();
            isShow = false;
        };
        popup.autoClearWhenClick = true;
        // BackgroundUI.Instance.Show(popup.gameObject, null);
    }


    void ShowUnsavedDataPopup()
    {
        // === Tạo canvas popup ===
        GameObject popupGO = new GameObject("UnsavedDataPopup");
        popupGO.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        popupGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        popupGO.AddComponent<GraphicRaycaster>();

        // === Tạo background mờ ===
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(popupGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.6f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // === Tạo panel ===
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(bgGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = Color.white;
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 300);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        // === Tạo text ===
        GameObject textGO = new GameObject("Message");
        textGO.transform.SetParent(panelGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = "Dữ liệu của bạn chưa được lưu!\nNếu thoát ra sẽ mất dữ liệu!";
        // text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // === Tạo nút OK ===
        GameObject okGO = CreateButton("OK", panelGO.transform, new Vector2(-60, -60));
        okGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("OK clicked");
            OnClickYes();
            Destroy(popupGO);
            isShow = false;
        });

        // === Tạo nút Cancel ===
        GameObject cancelGO = CreateButton("Cancel", panelGO.transform, new Vector2(60, -60));
        cancelGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            isShow = false;
            Debug.Log("Cancel clicked");
            Destroy(popupGO);
        });
    }

    private static void OnClickYes()
    {
        RoomStorage.rooms.Clear();
        SceneHistoryManager.LoadPreviousScene();
    }

    GameObject CreateButton(string label, Transform parent, Vector2 anchoredPos)
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
        // txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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