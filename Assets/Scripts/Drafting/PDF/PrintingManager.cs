using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using SimpleFileBrowser;
using System.Text;

public class PrintingManager : MonoBehaviour
{
    public Button btnExportPDF; // Nút xuất PDF
    public GameObject SuccessPanel; // Panel thông báo xuất thành công
    public GameObject ErrorPanel; // Panel thông báo lỗi xuất PDF
    public string unit = "m";

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
        SavePdfToDownloads(pdfBytes, "Drawing_Tester_House.pdf");
    }

    public void SavePdfToDownloads(byte[] pdfData, string fileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
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
#else
        // Editor / non-Android fallback
        string fallbackPath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(fallbackPath, pdfData);
        Debug.Log("Saved locally (Editor): " + fallbackPath);
#endif
    }

    void ExPDFx()
    {
        PdfHouseExporter.ExportHousePDF();
    }
}
