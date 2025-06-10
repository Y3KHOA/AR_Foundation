using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject panelHome;

    [Header("Buttons")]
    public GameObject buttonStart;
    public GameObject buttonCancel;
    public GameObject buttonAbout;

    [Header("Objects")]
    public GameObject objectStart;
    public GameObject objectDraw;
    public GameObject objectCreateNew;
    public GameObject objectMyDrawing;
    public GameObject objectByCamera;
    public GameObject objectUnit;
    public GameObject objectPanelRecords;
    public GameObject objecHome;

    [Header("Panels")]
    public GameObject panelAbout;

    void Start()
    {        
        panelHome.SetActive(true);

        ResetUIToInitialState();
        // Gán sự kiện click cho nút nếu cần (nếu không dùng UnityEvent trong Inspector)
        buttonStart.GetComponent<Button>().onClick.AddListener(OnStartPressed);
        buttonCancel.GetComponent<Button>().onClick.AddListener(OnCancelPressed);
        buttonAbout.GetComponent<Button>().onClick.AddListener(OnAboutPressed);
    }

    public void OnStartPressed()
    {
        buttonStart.SetActive(false);
        buttonCancel.SetActive(true);

        // Tùy chọn: mở các object khác khi bắt đầu
        // panelHome.SetActive(false);
        objectStart.SetActive(true);
        objectDraw.SetActive(false);
        objectCreateNew.SetActive(false);
        objectMyDrawing.SetActive(false);
        objectByCamera.SetActive(false);
        objectUnit.SetActive(false);
        objectPanelRecords.SetActive(false);
    }

    public void OnCancelPressed()
    {
        ResetUIToInitialState();
    }
    
    public void ResetUIToInitialState()
    {
        // Reset các button
        buttonStart.SetActive(true);
        buttonCancel.SetActive(false);

        // Ẩn toàn bộ các object hoạt động
        panelHome.SetActive(true);
        objectStart.SetActive(false);
        objectDraw.SetActive(false);
        objectCreateNew.SetActive(false);
        objectMyDrawing.SetActive(false);
        objectByCamera.SetActive(false);
        objectUnit.SetActive(false);
        objectPanelRecords.SetActive(false);
    }

    // Tuỳ chọn: mở/đóng panel giới thiệu
    public void OnAboutPressed()
    {
        bool isActive = !panelAbout.activeSelf;
        panelAbout.SetActive(isActive);
        objecHome.SetActive(!isActive); // nếu panelAbout bật thì objecHome tắt
        // panelMid.SetActive(!isActive);
    }
}
