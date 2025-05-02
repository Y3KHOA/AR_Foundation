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
    public CheckpointManager checkpointManager; // Quản lý các checkpoint
    public GameObject SuccessPanel; // Panel thông báo xuất thành công
    public GameObject ErrorPanel; // Panel thông báo lỗi xuất PDF
    public string unit = "m";
    string path;

    void Start()
    {
        // btnExportPDF.onClick.AddListener(() =>
        // {
        //  StartCoroutine(RequestPermissionAndExport());
        // });
        btnExportPDF.onClick.AddListener(() =>
        {
        ExportAllDrawingsAndSaveToDownloads(); // Gọi hàm mới
        });
    }

    public void ExportAllDrawingsToPDF(string exportPath)
    {
        List<List<Vector2>> allPolygons = new List<List<Vector2>>();
        List<List<float>> allDistances = new List<List<float>>();


        foreach (var checkpointLoop in checkpointManager.AllCheckpoints)
        {
            if (checkpointLoop == null || checkpointLoop.Count < 2)
                continue;

            List<Vector2> polygon = new List<Vector2>();
            List<float> distances = new List<float>();

            for (int i = 0; i < checkpointLoop.Count; i++)
            {
                Vector3 pos = checkpointLoop[i].transform.position;
                polygon.Add(new Vector2(pos.x, pos.z));

                if (i > 0)
                {
                    Vector3 prev = checkpointLoop[i - 1].transform.position;
                    distances.Add(Vector3.Distance(prev, pos));
                }
            }

            // Xử lý: nếu polygon khép kín và điểm cuối == điểm đầu → loại bỏ điểm cuối
            if (polygon.Count > 2 && Vector2.Distance(polygon[0], polygon[^1]) < 0.01f)
            {
                polygon.RemoveAt(polygon.Count - 1);

                // Nếu distances dư 1 phần tử thì cũng cần xóa
                if (distances.Count == polygon.Count + 1)
                    distances.RemoveAt(distances.Count - 1);
            }

            allPolygons.Add(polygon);
            allDistances.Add(distances);
        }

        Debug.Log("Export PDF to: " + exportPath);
        path = exportPath;
        Debug.Log("Path PDF: " + path);

        try
        {
            List<WallLine> allWallLines = new List<WallLine>();
            foreach (var wall in checkpointManager.wallLines)
            {
                Vector2 start = new Vector2(wall.start.x, wall.start.z);
                Vector2 end = new Vector2(wall.end.x, wall.end.z);
                allWallLines.Add(new WallLine(start, end, wall.type));
                Debug.Log("Add WallLine type for list -> PDF: " + wall.type);
            }

            // PdfExporter.ExportMultiplePolygonsToPDF(allPolygons, allWallLines, path, 0.1f);
            byte[] pdfBytes = PdfExporter.GeneratePdfAsBytes(allPolygons, allWallLines, 0.1f);
            FileBrowserHelpers.WriteBytesToFile(path, pdfBytes);

            Debug.Log("PDF exported to: " + path);

            if (SuccessPanel != null)
                SuccessPanel.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Export PDF failed: " + ex.Message);

            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }

    IEnumerator RequestPermissionAndExport()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("PDF Files", ".pdf"));
        FileBrowser.SetDefaultFilter(".pdf");

        Debug.Log("Opening FileBrowser.WaitForSaveDialog...");
        yield return FileBrowser.WaitForSaveDialog(
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: null,
            initialFilename: "Drawing_Tester_House.pdf",
            title: "Save PDF File");

        Debug.Log("FileBrowser.Success = " + FileBrowser.Success);
        Debug.Log("FileBrowser.Result.Length = " + FileBrowser.Result.Length);

        if (FileBrowser.Success && FileBrowser.Result.Length > 0)
        {
            string selectedFilePath = FileBrowser.Result[0];
            Debug.Log("File selected: " + selectedFilePath);
            ExportAllDrawingsToPDF(selectedFilePath);
        }
        else
        {
            Debug.LogError("No file selected.");
            if (ErrorPanel != null)
                ErrorPanel.SetActive(true);
        }
    }

    public void ExportAllDrawingsAndSaveToDownloads()
    {
        List<List<Vector2>> allPolygons = new List<List<Vector2>>();
        List<WallLine> allWallLines = new List<WallLine>();

        foreach (var checkpointLoop in checkpointManager.AllCheckpoints)
        {
            if (checkpointLoop == null || checkpointLoop.Count < 2)
                continue;

            List<Vector2> polygon = new List<Vector2>();
            for (int i = 0; i < checkpointLoop.Count; i++)
            {
                Vector3 pos = checkpointLoop[i].transform.position;
                polygon.Add(new Vector2(pos.x, pos.z));
            }

            // Remove duplicate end-point if loop is closed
            if (polygon.Count > 2 && Vector2.Distance(polygon[0], polygon[^1]) < 0.01f)
                polygon.RemoveAt(polygon.Count - 1);

            allPolygons.Add(polygon);
        }

        foreach (var wall in checkpointManager.wallLines)
        {
            Vector2 start = new Vector2(wall.start.x, wall.start.z);
            Vector2 end = new Vector2(wall.end.x, wall.end.z);
            allWallLines.Add(new WallLine(start, end, wall.type));
        }

        byte[] pdfBytes = PdfExporter.GeneratePdfAsBytes(allPolygons, allWallLines, 0.1f);
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
}
