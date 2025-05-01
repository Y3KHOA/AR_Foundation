using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
        btnExportPDF.onClick.AddListener(ExportAllDrawingsToPDF);
        btnExportPDF.onClick.AddListener(ExportScene);
        btnExportPDF.onClick.AddListener(ExPDFx);
    }

    public void ExportAllDrawingsToPDF()
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

        // string path = Path.Combine("/storage/emulated/0/Download", "XHeroScan/PDF/Drawing_All_Test1.pdf");
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // Trên PC
        string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/Downloads";
        path = Path.Combine(downloadsPath, "XHeroScan/PDF/Drawing_Tester_House.pdf");
#else
    // Trên Android
    path = Path.Combine("/storage/emulated/0/Download", "XHeroScan/PDF/Drawing_Tester_House.pdf");
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#endif
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

            PdfExporter.ExportMultiplePolygonsToPDF(allPolygons, allWallLines, path, 0.1f);
            // PdfExporter.ExportMultiplePolygonsToPDF(allPolygons, path, 0.1f);

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
    void ExportScene()
    {
        string outputPath = "/storage/emulated/0/Download/XHeroScan/PDF/Drawing_All_Test2.pdf";
        ImageExporter.CaptureAndExport(Camera.main, 1024, 768, "scene.png", outputPath, true);
    }
    void ExPDFx()
    {
        PdfHouseExporter.ExportHousePDF();
    }
}
