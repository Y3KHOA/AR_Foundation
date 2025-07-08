using UnityEngine;
using TMPro;

public class TMPNumericInputField : MonoBehaviour
{
    public TMP_InputField tmpInputField;

    void Awake()
    {
        if (tmpInputField != null)
        {
            tmpInputField.contentType = TMP_InputField.ContentType.IntegerNumber; 
            tmpInputField.keyboardType = TouchScreenKeyboardType.NumberPad;
            tmpInputField.ForceLabelUpdate();
        }
    }
}
