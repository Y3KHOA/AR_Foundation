//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class GroundCanvas : MonoBehaviour
//{
//    public Ground ground;
//    public Image groundImage;
//    public TextMeshProUGUI groundNameText;
    
//    public void LoadData()
//    {
//        LoadImage(ground.imageName);
//        groundNameText.text = ground.groundName;
//    }

//    private void LoadImage(string imageName)
//    {
//        // Tải hình ảnh từ thư mục Resources
//        Sprite sprite = Resources.Load<Sprite>($"ImagesGround/{imageName}");
//        if (sprite != null)
//        {
//            groundImage.sprite = sprite;
//        }
//        else
//        {
//            Debug.LogError($"Failed to load image '{imageName}'");
//            groundImage.sprite = null;
//        }
//    }
//}
