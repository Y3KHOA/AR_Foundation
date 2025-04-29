using UnityEngine;

public class ToggleContent : MonoBehaviour
{
    // Kéo ContentPanel vào đây trong Inspector
    public GameObject contentPanel1;
    public GameObject contentPanel2;

    private bool isVisible = true;

    public void Toggle()
    {
        isVisible = !isVisible;
        contentPanel1.SetActive(isVisible);
        contentPanel2.SetActive(!isVisible);
    }
}
