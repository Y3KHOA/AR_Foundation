using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Import TextMeshPro
using System.Collections.Generic;

public class BtnUnit2D : MonoBehaviour
{
    public Button btnEnter;
    public TMP_InputField heightInput; // Sử dụng TMP_InputField
    public TMP_Dropdown unitDropdown; // Dropdown để chọn đơn vị
    
    public GameObject ErrorPanel; // Panel thông báo lỗi

    void Start()
    {
        if (heightInput == null)
        {
            Debug.LogError("Chưa gán TMP_InputField cho BtnUnit!");
        }
        if (unitDropdown == null)
        {
            Debug.LogError("Chưa gán TMP_Dropdown cho BtnUnit!");
        }
        if (btnEnter != null)
            btnEnter.onClick.AddListener(OnBtnUnitClicked);
    }

    public void OnBtnUnitClicked()
    {
        string inputText = heightInput.text.Trim();

        // Nếu chứa dấu chấm (.), báo lỗi vì chỉ chấp nhận dấu phẩy (,)
        if (inputText.Contains("."))
        {
            Debug.LogWarning("Chỉ chấp nhận định dạng với dấu phẩy (,) thay vì dấu chấm (.)");
            ErrorPanel.SetActive(true);
            return;
        }

        // Thử chuyển dấu phẩy thành dấu chấm tạm thời để có thể parse
        string normalizedInput = inputText.Replace(',', '.');

        // Kiểm tra parse thành số và số đó phải dương
        if (!float.TryParse(normalizedInput, out float heightValue) || heightValue <= 0)
        {
            Debug.LogWarning("Giá trị không hợp lệ: phải là số dương và không chứa chữ");
            ErrorPanel.SetActive(true);
            return;
        }

        ErrorPanel.SetActive(false); // Ẩn panel lỗi nếu hợp lệ

        // Lấy đơn vị từ Dropdown
        string selectedUnit = unitDropdown.options[unitDropdown.value].text;

        // Chuyển đổi chiều cao theo đơn vị đo đã chọn
        float convertedHeight = ConvertHeightToUnit(heightValue, selectedUnit);

        // Lưu chiều cao đã chuyển đổi vào PlayerPrefs
        PlayerPrefs.SetFloat("HeightValue", convertedHeight);
        PlayerPrefs.SetString("SelectedUnit", selectedUnit);
        PlayerPrefs.Save();

        Debug.Log($"Giá trị nhập vào: {inputText} {selectedUnit}, giá trị sau khi chuyển đổi: {convertedHeight}");

        // Gán chiều cao cho các Room hiện có
        List<Room> newRoom = RoomStorage.rooms;
        foreach (Room room in newRoom)
        {
            room.heights.Clear();
            for (int i = 0; i < room.checkpoints.Count; i++)
            {
                room.heights.Add(convertedHeight);
            }
            Debug.Log($"Gán chiều cao {convertedHeight} cho Room có {room.checkpoints.Count} điểm.");
        }

        ButtonOk.IsOkButtonShown = true;
        // Chuyển scene
        SceneManager.LoadScene("FlatExampleScene");
    }


    // Hàm chuyển đổi chiều cao theo đơn vị
    float ConvertHeightToUnit(float height, string unit)
    {
        switch (unit)
        {
            case "cm": return height / 100f; // Chuyển cm về mét
            case "m": return height * 1f; // Chuyển m về mét
            case "inch": return height / 39.3701f; // Chuyển inch về mét
            case "ft": return height / 3.28084f; // Chuyển feet về mét
            default: return height; // Mặc định là mét
        }
    }
}
