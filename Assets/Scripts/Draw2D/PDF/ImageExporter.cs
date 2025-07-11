using UnityEngine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class ImageExporter
{
    /// <summary>
    /// Chụp ảnh từ một camera
    /// </summary>
    public static Texture2D CaptureFromCamera(Camera cam, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);

        return screenshot;
    }

    /// <summary>
    /// Lưu Texture2D thành file PNG
    /// </summary>
    public static string SaveTextureToPNG(Texture2D texture, string filename)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        Debug.Log("Ảnh PNG đã được lưu tại: " + path);
        return path;
    }

    public static void CreatePdfFromImage(string imagePath, string fullPdfPath)
    {
        string dir = Path.GetDirectoryName(fullPdfPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (FileStream fs = new FileStream(fullPdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Document doc = new Document();
            PdfWriter.GetInstance(doc, fs);
            doc.Open();

            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imagePath);
            image.Alignment = Element.ALIGN_CENTER;

            float pageWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
            float pageHeight = doc.PageSize.Height - doc.TopMargin - doc.BottomMargin;
            image.ScaleToFit(pageWidth, pageHeight);

            doc.Add(image);
            doc.Close();
        }

        Debug.Log("PDF đã được tạo tại: " + fullPdfPath);
    }


    /// <summary>
    /// Chụp từ camera, lưu PNG, và tạo PDF luôn (nếu muốn)
    /// </summary>
    public static void CaptureAndExport(Camera cam, int width, int height, string imageName = "capture.png", string pdfName = "capture.pdf", bool generatePdf = true)
    {
        Texture2D image = CaptureFromCamera(cam, width, height);
        string imgPath = SaveTextureToPNG(image, imageName);

        if (generatePdf)
        {
            CreatePdfFromImage(imgPath, pdfName);
        }
    }
}
