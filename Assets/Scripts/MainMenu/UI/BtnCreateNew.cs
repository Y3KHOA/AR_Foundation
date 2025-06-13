using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BtnCreateNew:MonoBehaviour
{
    public Button ButtonNewDrawing;
    public Button ButtonRecords;
    public GameObject PanelRecords;      // GameObject sẽ được bật khi nhấn Enter
    public GameObject buttonBar; // GameObject chứa các button cần ẩn
    private static bool isMeasure = false;
    public bool IsMeasure { set { isMeasure = value; } get { return isMeasure; } }
    public static btnByCam Instance;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    void Start()
    {
        if (ButtonNewDrawing != null)
            ButtonNewDrawing.onClick.AddListener(OnButtonNewDrawingClicked);
        else
            Debug.LogError("btnEnter chưa được gán!");

        if (ButtonRecords != null)
            ButtonRecords.onClick.AddListener(OnButtonRecordsClicked);
        else
            Debug.LogError("btnMeasure chưa được gán!");

        if (PanelRecords != null)
            PanelRecords.SetActive(false);
        else
            Debug.LogError("unit chưa được gán!");

        if (buttonBar == null)
            Debug.LogError("buttonBar chưa được gán!");
    }

    void OnButtonNewDrawingClicked()
    {
        isMeasure = true;
        Debug.Log("NewDrawing clicked - To scene SampleScene\nNewDrawing =true");
        SceneManager.LoadScene("SampleScene"); // Chuyển scene
    }

    void OnButtonRecordsClicked()
    {
        if (PanelRecords != null)
            PanelRecords.SetActive(true); // Hiện GameObject unit

        if (buttonBar != null)
            buttonBar.SetActive(false); // Ẩn toàn bộ Button Bar
    }
}
