using TMPro;
using UnityEngine;

public class InputConfig : MonoBehaviour
{
    public TMP_InputField inputField;
    public string valueTemp = "0";

    private void Start()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        valueTemp = "0";
    }
}
