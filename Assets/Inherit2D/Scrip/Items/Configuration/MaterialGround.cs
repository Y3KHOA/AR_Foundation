using UnityEngine;

/// <summary>
/// Lớp đại diện cho vật liệu mặt đất trong trò chơi.
/// </summary>
public class MaterialGround : MonoBehaviour
{
    public string nameMaterial;
    public string image;

    public MaterialGround()
    {
        nameMaterial = "";
        image = "";
    }

    public MaterialGround(string nameMaterial, string image)
    {
        this.nameMaterial = nameMaterial;
        this.image = image;
    }
}
