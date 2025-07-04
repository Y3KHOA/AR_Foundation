using UnityEngine;
using UnityEngine.UI;

public class BtnChoose1 : MonoBehaviour
{
    public Button button1;
    public Button button2;

    public GameObject panel1;
    public GameObject panel2;
    public GameObject panelCore;
    public GameObject panelLoadFile;

    void Start()
    {
        if (button1 != null)
            button1.onClick.AddListener(OnButton1Clicked);

        if (button2 != null)
            button2.onClick.AddListener(OnButton2Clicked);

        // Ẩn cả hai panel lúc đầu (hoặc tùy ý set cái nào hiện)
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);
    }

    void OnButton1Clicked()
    {
        if (panel1 != null) panel1.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);
        if (panelLoadFile != null) panelLoadFile.SetActive(false);

        if (panelCore != null) panelCore.SetActive(false);
    }

    void OnButton2Clicked()
    {
        if (panel2 != null) panel2.SetActive(true);
        if (panel1 != null) panel1.SetActive(false);
        if (panelLoadFile != null) panelLoadFile.SetActive(false);
        
        if (panelCore != null) panelCore.SetActive(false);
    }
}
