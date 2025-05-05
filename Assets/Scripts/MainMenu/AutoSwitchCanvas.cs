using UnityEngine;

public class AutoSwitchCanvas : MonoBehaviour
{
    public GameObject canvasPhone;
    public GameObject canvasTablet;

    void Start()
    {
        float aspect = (float)Screen.width / (float)Screen.height;

        if (aspect > 0.7f)
        {
            // Tablet: thường aspect khoảng 0.75 (4:3)
            canvasPhone.SetActive(false);
            canvasTablet.SetActive(true);
        }
        else
        {
            // Phone: aspect dài hơn 9:16 (~0.56)
            canvasPhone.SetActive(true);
            canvasTablet.SetActive(false);
        }
    }
}
