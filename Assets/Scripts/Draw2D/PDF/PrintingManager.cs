using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

public class PrintingManager : MonoBehaviour
{
    public Button btnExportPDF; // Nút xuất PDF
    public GameObject SuccessPanel; // Panel thông báo xuất thành công
    public GameObject ErrorPanel; // Panel thông báo lỗi xuất PDF
    public string unit = "m";

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _SavePdfToDocuments(byte[] pdfData, int dataLength, string fileName);
    
    [DllImport("__Internal")]
    private static extern void _OpenPdfWithActivityViewController(string filePath);
#endif

    void Start()
    {
        btnExportPDF.onClick.AddListener(() =>
        {
            ExportAllDrawingsAndSaveToDownloads(); // Gọi hàm mới
        });
    }

    public void ExportAllDrawingsAndSaveToDownloads()
    {
        // byte[] pdfBytes = PdfExporter.GeneratePdfAsBytes(allPolygons, allWallLines, 0.1f);
        byte[] pdfBytes = PdfExporter.GeneratePdfAsBytes(RoomStorage.rooms, 0.1f);
        // SavePdfToDownloads(pdfBytes, "Bản vẽ mẫu.pdf");
        // Tạo tên file theo ngày giờ: yyyyMMdd_HHmmss.pdf
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Bản vẽ 2D.pdf";

        SavePdfToDownloads(pdfBytes, fileName);
    }

    public void SavePdfToDownloads(byte[] pdfData, string fileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SavePdfToAndroidDownloads(pdfData, fileName);
#elif UNITY_IOS && !UNITY_EDITOR
        SavePdfToiOSDocuments(pdfData, fileName);
#else
        // Editor / non-mobile fallback
        string fallbackPath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(fallbackPath, pdfData);
        Debug.Log("Saved locally (Editor): " + fallbackPath);
#endif
    }

    private void SavePdfToAndroidDownloads(byte[] pdfData, string fileName)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

                AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Downloads");
                AndroidJavaObject contentValues = new AndroidJavaObject("android.content.ContentValues");

                contentValues.Call("put", "title", fileName);
                contentValues.Call("put", "_display_name", fileName);
                contentValues.Call("put", "mime_type", "application/pdf");
                contentValues.Call("put", "relative_path", "Download/XHeroScan/PDF");

                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                contentValues.Call("put", "date_added", new AndroidJavaObject("java.lang.Long", currentTime / 1000));
                contentValues.Call("put", "date_modified", new AndroidJavaObject("java.lang.Long", currentTime / 1000));

                AndroidJavaObject externalUri = mediaStore.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");

                AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", externalUri, contentValues);
                if (uri == null)
                {
                    Debug.LogError("MediaStore insert returned null.");
                    if (ErrorPanel != null)
                        ErrorPanel.SetActive(true);
                    return;
                }

                // Open output stream
                AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
                if (outputStream == null)
                {
                    Debug.LogError("Cannot open output stream.");
                    return;
                }

                // Write bytes
                outputStream.Call("write", pdfData);
                outputStream.Call("flush");
                outputStream.Call("close");

                // Mở PDF sau khi lưu
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/pdf");
                intent.Call<AndroidJavaObject>("addFlags", new AndroidJavaClass("android.content.Intent").GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));

                // Hiển thị chooser "Open With"
                AndroidJavaObject chooser = new AndroidJavaClass("android.content.Intent").CallStatic<AndroidJavaObject>(
                    "createChooser", intent, "Open PDF with..."
                );

                activity.Call("startActivity", chooser);

                Debug.Log("PDF saved to Downloads using MediaStore.");
                if (SuccessPanel != null)
                    SuccessPanel.SetActive(true);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save PDF using MediaStore: " + ex.Message);
            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }

    private void SavePdfToiOSDocuments(byte[] pdfData, string fileName)
    {
#if UNITY_IOS && !UNITY_EDITOR
        try
        {
            Debug.Log("Attempting to save PDF on iOS: " + fileName);
            
            // Check if native plugin is available
            if (pdfData == null || pdfData.Length == 0)
            {
                Debug.LogError("PDF data is null or empty");
                if (ErrorPanel != null)
                    ErrorPanel.SetActive(true);
                return;
            }
            
            // Save PDF to iOS Documents directory using native plugin
            _SavePdfToDocuments(pdfData, pdfData.Length, fileName);
            
            Debug.Log("PDF saved to iOS Documents directory: " + fileName);
            if (SuccessPanel != null)
                SuccessPanel.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save PDF on iOS: " + ex.Message);
            Debug.LogError("Stack trace: " + ex.StackTrace);
            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
#else
        // Fallback for iOS Editor testing
        Debug.Log("iOS Editor mode - saving to persistent data path");
        string fallbackPath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            File.WriteAllBytes(fallbackPath, pdfData);
            Debug.Log("PDF saved to: " + fallbackPath);
            if (SuccessPanel != null)
                SuccessPanel.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save PDF in fallback mode: " + ex.Message);
            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
#endif
    }

    void ExPDFx()
    {
        PdfHouseExporter.ExportHousePDF();
    }
}