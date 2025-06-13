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
        // Kiểm tra input có hợp lệ không
        if (!float.TryParse(heightInput.text, out float heightValue))
        {
            Debug.LogError("Value invalid!");
            return;
        }

        // Lấy đơn vị từ Dropdown
        string selectedUnit = unitDropdown.options[unitDropdown.value].text;

        // Chuyển đổi chiều cao theo đơn vị đo đã chọn
        float convertedHeight = ConvertHeightToUnit(heightValue, selectedUnit);

        // Lưu chiều cao đã chuyển đổi vào PlayerPrefs
        PlayerPrefs.SetFloat("HeightValue", convertedHeight);
        PlayerPrefs.SetString("SelectedUnit", selectedUnit); // Bạn có thể vẫn lưu đơn vị này nếu cần
        PlayerPrefs.Save();

        Debug.Log($"Giá trị nhập vào: {heightValue} {selectedUnit}, giá trị sau khi chuyển đổi: {convertedHeight}");

        // Lưu vào Room
        List<Room> newRoom = RoomStorage.rooms;
        foreach (Room room in newRoom)
        {
            room.heights.Clear(); // Xoá cũ nếu đã từng gán
            for (int i = 0; i < room.checkpoints.Count; i++)
            {
                room.heights.Add(convertedHeight);
            }

            Debug.Log($"Gán chiều cao {convertedHeight} cho Room có {room.checkpoints.Count} điểm.");
        }

        // Chuyển sang scene FlatExampleScene
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
