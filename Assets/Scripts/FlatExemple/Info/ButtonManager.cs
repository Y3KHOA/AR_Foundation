using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnFloor;
    public Button btn3D;
    public Button btnInfo;

    [Header("Buttons Child")]
    public Button btnEdit;
    public Button btnSave;

    [Header("Panels")]
    public GameObject panelFloor;
    public GameObject panel3D;
    public GameObject panelInfo;
    public GameObject previewTexture;
    public GameObject Draw2D;

    private void Start()
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(false);
        panel3D.SetActive(false);
        panelInfo.SetActive(false);

        Draw2D.gameObject.SetActive(false);

        if (ButtonOk.IsOkButtonShown)
        {
            previewTexture.SetActive(true);
            panelFloor.SetActive(false);
            panel3D.SetActive(true);
            panelInfo.SetActive(false);
        }
        // Gán sự kiện click sau khi setup UI
        if (btnFloor != null)
            btnFloor.onClick.AddListener(() => OnButtonFloor(panelFloor));
        if (btnEdit != null)
            btnEdit.onClick.AddListener(() => StartCoroutine(SafeLoadScene("DraftingScene")));
        if (btn3D != null)
            btn3D.onClick.AddListener(() => OnButton3D(panel3D));
        if (btnInfo != null)
            btnInfo.onClick.AddListener(() => OnButtonInfo(panelInfo));
    }

    private void OnButtonFloor(GameObject selectedPanel)
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(true);
        panel3D.SetActive(false);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(true);
        btnSave.gameObject.SetActive(false);
        Draw2D.gameObject.SetActive(true);
    }
    private void OnButton3D(GameObject selectedPanel)
    {
        previewTexture.SetActive(true);
        panelFloor.SetActive(false);
        panel3D.SetActive(true);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(false);
        btnSave.gameObject.SetActive(false);
        Draw2D.gameObject.SetActive(false);
    }
    private void OnButtonInfo(GameObject selectedPanel)
    {
        previewTexture.SetActive(false);
        panelFloor.SetActive(false);
        panel3D.SetActive(false);
        panelInfo.SetActive(true);
        btnEdit.gameObject.SetActive(false);
        btnSave.gameObject.SetActive(true);
        Draw2D.gameObject.SetActive(false);
    }

    IEnumerator SafeLoadScene(string sceneName)
    {
        Debug.Log("Button clicked - Cleaning pointer state...");

        // Tắt pointer state hiện tại
        var eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            // Huỷ chọn đối tượng UI hiện tại (nếu đang focus hoặc được highlight)
            eventSystem.SetSelectedGameObject(null);

            // Nếu đang dùng InputSystemUIInputModule, có thể disable 1 frame để tránh xử lý pointer đang active
            var inputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule != null)
                inputModule.enabled = false;
        }

        // Chờ 1 frame để mọi pointer state clear
        yield return null;

        // Bật lại InputModule nếu cần (tuỳ game bạn có dùng tiếp hay không)
        var reInputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (reInputModule != null)
            reInputModule.enabled = true;

        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
