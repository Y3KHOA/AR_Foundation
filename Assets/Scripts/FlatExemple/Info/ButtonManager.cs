using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    public ToggleButtonUI btnFloor;

    public ToggleButtonUI btn3D;
    public ToggleButtonUI btnInfo;

    [Header("Buttons Child")]
    public Button btnEdit;

    public Button btnSave;

    [Header("Panels")]
    public GameObject panelFloor;

    public GameObject panel3D;
    public GameObject panelInfo;
    public GameObject previewTexture;
    public GameObject Draw2D;

    [Header("PreviewCamera")]
    public Camera PreviewCamera;

    [SerializeField] private List<ToggleButtonUI> togglesButtonList = new();

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

        AssignButtonEvent();

        InitToggleButton();
    }

    private void AssignButtonEvent()
    {
        // Gán sự kiện click sau khi setup UI
        if (btnFloor != null)
            btnFloor.ActiveEvent = () => OnButtonFloor(panelFloor);
        if (btnEdit != null)
            btnEdit.onClick.AddListener(() => StartCoroutine(SafeLoadScene("DraftingScene")));
        if (btn3D != null)
            btn3D.ActiveEvent = () => OnButton3D(panel3D);
        if (btnInfo != null)
            btnInfo.ActiveEvent = () => OnButtonInfo(panelInfo);
    }

    private void InitToggleButton()
    {
        foreach (var item in togglesButtonList)
        {
            item.btn.onClick.AddListener(() => { OnClickBtn(item); });
        }

        // set default button 
        OnClickBtn(btnFloor);
    }

    private ToggleButtonUI currentButton;

    private void OnClickBtn(ToggleButtonUI toggleButtonUI)
    {
        foreach (var item in togglesButtonList)
        {
            var state = item == toggleButtonUI ? ToggleButtonUI.State.Active : ToggleButtonUI.State.DeActive;
            if (item == toggleButtonUI)
            {
                if (item == currentButton) break;
                // set current button
                currentButton = toggleButtonUI;
                currentButton.OnActive();
            }
            
            item.ChangeState(state);
        }
    }

    private void OnButtonFloor(GameObject selectedPanel)
    {
        previewTexture.SetActive(true);
        panelFloor.SetActive(true);
        panel3D.SetActive(false);
        panelInfo.SetActive(false);
        btnEdit.gameObject.SetActive(true);
        btnSave.gameObject.SetActive(false);
        Draw2D.gameObject.SetActive(true);

        if (PreviewCamera != null)
        {
            PreviewCamera.transform.position = new Vector3(0f, 10f, 0f);
            PreviewCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Debug.Log($"[CameraFloorPlan] position: {PreviewCamera.transform.position}, rotation: {PreviewCamera.transform.rotation}");
        }
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

        if (PreviewCamera != null)
        {
            PreviewCamera.transform.position = new Vector3(0f, 0f, -10f);
            PreviewCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Debug.Log($"[Camera3D] position: {PreviewCamera.transform.position}, rotation: {PreviewCamera.transform.rotation}");
        }
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

        if (PreviewCamera != null)
        {
            PreviewCamera.transform.position = new Vector3(0f, 0f, -10f);
            PreviewCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Debug.Log("[Camera] Info mode");
        }
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
            // var inputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(); //cũ rồi
            var inputModule = Object.FindFirstObjectByType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        
            if (inputModule != null)
                inputModule.enabled = false;
        }

        // Chờ 1 frame để mọi pointer state clear
        yield return null;

        // Bật lại InputModule nếu cần (tuỳ game bạn có dùng tiếp hay không)
        // var reInputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        var reInputModule = Object.FindFirstObjectByType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        
        if (reInputModule != null)
            reInputModule.enabled = true;

        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}