using System.Collections;
using System.IO;
using UnityEngine;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class PrintingManager : MonoBehaviour
{
    string path = null;

    void Start()
    {
        // Ghi file vào thư mục Download/ARK (trên Android)
#if UNITY_ANDROID && !UNITY_EDITOR
        string folderPath = "/storage/emulated/0/Download/ARK";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        path = Path.Combine(folderPath, "Ticket.pdf");
#else
        // Với PC/Editor thì dùng đường dẫn tạm
        path = Application.persistentDataPath + "/Ticket.pdf";
#endif

        Debug.Log("PDF sẽ được lưu tại: " + path);
    }

    public void GenerateFile()
    {
        if (File.Exists(path))
            File.Delete(path);

        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            var document = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            var writer = PdfWriter.GetInstance(document, fileStream);

            document.Open();
            document.NewPage();

            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Paragraph p = new Paragraph("Ticket Id : 12345");
            p.Alignment = Element.ALIGN_CENTER;
            document.Add(p);

            p = new Paragraph("Bet Number : 1     BetAmount : 100");
            p.Alignment = Element.ALIGN_CENTER;
            document.Add(p);

            document.Close();
            writer.Close();
        }

        Debug.Log("PDF đã được tạo tại: " + path);
    }
}
