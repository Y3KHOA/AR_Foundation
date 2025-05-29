using TMPro;
using UnityEngine;

/// <summary>
/// Lớp này quản lý việc hiển thị thông tin cho các vật phẩm trong trò chơi.
/// </summary>
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
