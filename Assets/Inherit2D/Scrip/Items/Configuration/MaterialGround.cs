using UnityEngine;

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
