using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureInverter : EditorWindow
{
    Texture2D sourceTexture;

    [MenuItem("Tools/Invert Icon Colors")]
    public static void ShowWindow()
    {
        GetWindow<TextureInverter>("Invert Icon Colors");
    }

    void OnGUI()
    {
        GUILayout.Label("Chọn icon PNG cần đảo màu (đen → trắng)", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Icon PNG", sourceTexture, typeof(Texture2D), false);

        if (sourceTexture != null && GUILayout.Button("Invert và Lưu"))
        {
            InvertAndSave(sourceTexture);
        }
    }

    void InvertAndSave(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            // Đảo màu: đen thành trắng, trắng thành đen, giữ alpha
            Color c = pixels[i];
            c.r = 1f - c.r;
            c.g = 1f - c.g;
            c.b = 1f - c.b;
            pixels[i] = new Color(c.r, c.g, c.b, c.a);
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        // Lưu file mới
        byte[] pngData = newTexture.EncodeToPNG();
        string newPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_inverted.png";
        File.WriteAllBytes(newPath, pngData);
        AssetDatabase.Refresh();

        Debug.Log("Đã lưu ảnh invert tại: " + newPath);
    }
}
