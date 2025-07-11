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
            tmpInputField.shouldHideMobileInput = true;
        }
    }

    void Start()
    {
        StartCoroutine(DelayedForceFocus());
    }

    private System.Collections.IEnumerator DelayedForceFocus()
    {
        yield return null; // Chờ 1 frame
        ForceFocus();
    }

    public void ForceFocus()
    {
        if (tmpInputField != null)
        {
            tmpInputField.Select();
            tmpInputField.ActivateInputField();
        }
    }

    // void Update()
    // {
    //     Debug.Log("Focused? " + tmpInputField.isFocused);
    // }
}
