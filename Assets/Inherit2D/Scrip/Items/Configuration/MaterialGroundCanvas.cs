using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp này xử lý giao diện người dùng bật lên trong trò chơi, cho phép hiển thị tin nhắn hoặc thông báo.
/// </summary>
public class MaterialGroundCanvas : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public Image image;
    public GameObject selectedBorderGO;

    [HideInInspector] public MaterialGround materialGround;

    public void LoadData()
    {
        nameText.text = materialGround.nameMaterial;
        LoadImage(materialGround.image);
    }

    private void LoadImage(string imageName)
    {
        Sprite sprite = Resources.Load<Sprite>($"ImagesItem/MaterialsGround/{imageName}");
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogError($"Failed to load image '{imageName}'");
            image.sprite = null;
        }
    }
}
