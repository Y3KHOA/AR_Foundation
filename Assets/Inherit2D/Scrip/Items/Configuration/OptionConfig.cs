using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionConfig : MonoBehaviour
{
    public Button onBTN;
    public Button offBTN;
    public GameObject canvas;
    public bool isOpen = false;

    private void Start()
    {
        if(!isOpen)
        {
            onBTN.gameObject.SetActive(false);
            offBTN.gameObject.SetActive(true);
        }    
    }
}
