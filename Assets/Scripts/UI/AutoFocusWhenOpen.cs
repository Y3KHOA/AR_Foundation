using TMPro;
using UnityEngine;

public class AutoFocusWhenOpen : MonoBehaviour
{
    private TMP_InputField inputField;

    private void OnEnable()
    {
        if (!inputField)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (inputField)
        {
            Focus();
        }
    }

    public void Focus()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }
}