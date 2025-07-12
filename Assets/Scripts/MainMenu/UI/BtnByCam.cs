using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class btnByCam : MonoBehaviour
{
    public Button btnEnter;
    public Button btnMeasure;
    public GameObject unit;      // GameObject sẽ được bật khi nhấn Enter
    public GameObject buttonBar; // GameObject chứa các button cần ẩn
    public GameObject background; // GameObject chứa các button cần ẩn
    private static bool isMeasure = false;
    public bool IsMeasure { set { isMeasure = value; } get { return isMeasure; } }
    public static btnByCam Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (btnEnter != null)
            btnEnter.onClick.AddListener(OnEnterClicked);
        else
            Debug.LogError("btnEnter chưa được gán!");

        if (btnMeasure != null)
            btnMeasure.onClick.AddListener(OnMeasureClicked);
        else
            Debug.LogError("btnMeasure chưa được gán!");

        if (unit != null)
            unit.SetActive(false);
        else
            Debug.LogError("unit chưa được gán!");
        if (background != null)
            background.SetActive(false);
        else
            Debug.LogError("background chưa được gán!");

        if (buttonBar == null)
            Debug.LogError("buttonBar chưa được gán!");
    }

    void OnEnterClicked()
    {
        Debug.Log("Nhấn Enter - Ẩn các button và hiển thị unit");

        // if (btnEnter != null)
        //     btnEnter.gameObject.SetActive(false); // Ẩn nút Enter

        // if (btnMeasure != null)
        //     btnMeasure.gameObject.SetActive(false); // Ẩn nút Measure

        if (unit != null)
            unit.SetActive(true);    // Hiện GameObject unit
        
        if (background != null)
            background.SetActive(true); // Hiện GameObject background

        if (buttonBar != null)
            buttonBar.SetActive(false); // Ẩn toàn bộ Button Bar
    }

    void OnMeasureClicked()
    {
        isMeasure = true;
        Debug.Log("Measure clicked - To scene ARFoundation\nMeasure =true");
        SceneManager.LoadScene("AR"); // Chuyển scene
    }
}
