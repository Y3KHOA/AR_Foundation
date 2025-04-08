using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDropdownManager : MonoBehaviour
{
    public TMP_Dropdown unitDropdown;
    private string selectedUnit = "m";

    void Start()
    {
        unitDropdown.ClearOptions();
        unitDropdown.AddOptions(new System.Collections.Generic.List<string> { "m", "cm", "feet", "inch" });

        // Lấy đơn vị đã lưu trước đó
        selectedUnit = PlayerPrefs.GetString("SelectedUnit", "m");
        unitDropdown.value = unitDropdown.options.FindIndex(option => option.text == selectedUnit);

        unitDropdown.onValueChanged.AddListener(OnUnitChanged);
    }

    void OnUnitChanged(int index)
    {
        selectedUnit = unitDropdown.options[index].text;
    }

    // Gọi phương thức này khi nhấn Button
    public void ConfirmUnitSelection()
    {
        PlayerPrefs.SetString("SelectedUnit", selectedUnit);
        PlayerPrefs.Save();
        Debug.Log("Da luu don vi: " + selectedUnit);
    }
}
