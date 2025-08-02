using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SavePanelUI : MonoBehaviour
{
    // TODO: Thêm tính năng valid input trong tương lai cho file name

    private const string ErrorMessage_FileNameEmpty = "Tên file đang bị để trống";
    private const string ErrorMessage_FileNameExit = "Tên file đã tồn tại, vui lòng chọn tên khác";
    private const string SuccessMessage_ExportFileComplete = "Bạn đã lưu bản vẽ thành công";
    
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private TMP_InputField fileNameInputField;

    [SerializeField] private GameObject savePanelContainer;
    [SerializeField] private GameObject successPopup;
    [SerializeField] private GameObject failedPopup;

    private void Awake()
    {
        closeBtn.onClick.AddListener(Close);
        confirmBtn.onClick.AddListener(() => Show());
        confirmBtn.onClick.AddListener(() => Confirm());
        Close();
        successPopup.gameObject.SetActive(false);
        failedPopup.gameObject.SetActive(false);
    }

    private void Close()
    {
        savePanelContainer.gameObject.SetActive(false);
        BackgroundUI.Instance.Hide();
    }

    public void Show()
    {
        EventSystem.current.SetSelectedGameObject(fileNameInputField.gameObject);
        fileNameInputField.OnPointerClick(new PointerEventData(EventSystem.current)); 
        // BackgroundUI.Instance.Show(transform.gameObject, Close);
        savePanelContainer.gameObject.SetActive(true);
    }


    private void Confirm()
    {
        string fileName = fileNameInputField.text;

        bool isFileNameEmpty = string.IsNullOrEmpty(fileName);
        bool isFileExit = SaveLoadManager.DoesNameExist(fileName);

        if (isFileNameEmpty)
        {
            ShowErrorPopup(ErrorMessage_FileNameEmpty);
            return;
        }

        if (isFileExit)
        {
            ShowErrorPopup(ErrorMessage_FileNameExit);
            return;
        }


        ShowSuccessPopup(SuccessMessage_ExportFileComplete);        
        SaveLoadManager.Save(fileName);
        Close();
    }

    private void ShowSuccessPopup(string description)
    {
        successPopup.GetComponent<ToastUI>().DescriptionText = description;
        successPopup.gameObject.SetActive(true);
    }

    private void ShowErrorPopup(string description)
    {
        failedPopup.GetComponent<ToastUI>().DescriptionText = description;
        failedPopup.gameObject.SetActive(true);
    }
}