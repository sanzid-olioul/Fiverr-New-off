using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
namespace LancasterCreditCardDiversion.Services;

using Syncfusion.Drawing;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

public class ConvertAnyFileToPdf
{
    /// <summary>
    /// Convert a byte array representing a document or image into a PDF stream.
    /// </summary>
    /// <param name="inputBytes">The file content bytes.</param>
    /// <param name="fileName">Original file name (needed to detect extension/type).</param>
    /// <returns>A MemoryStream containing the PDF output.</returns>
    public async Task<MemoryStream> ConvertToPdfAsync(byte[] inputBytes, string fileName)
    {
        using var inputStream = new MemoryStream(inputBytes);
        inputStream.Position = 0;
        return await ConvertStreamToPdfAsync(inputStream, fileName);
    }

    private async Task<MemoryStream> ConvertStreamToPdfAsync(Stream inputStream, string fileName)
    {
        if (inputStream.CanSeek)
            inputStream.Position = 0;

        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        var output = new MemoryStream();

        switch (ext)
        {
            case ".pdf":
                // Already a PDF — just copy
                await inputStream.CopyToAsync(output);
                break;

            case ".doc":
            case ".docx":
            case ".rtf":
                ConvertWordStreamToPdf(inputStream, output, ext);
                break;
            case ".txt":
                ConvertTextStreamToPdf(inputStream, output);
                break;
            case ".xls":
            case ".xlsx":
            case ".csv":
                ConvertExcelStreamToPdf(inputStream, output, ext);
                break;

            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".bmp":
            case ".gif":
            case ".tif":
            case ".tiff":
                ConvertImageStreamToPdf(inputStream, output);
                break;

            default:
                throw new NotSupportedException($"Extension '{ext}' is not supported for Syncfusion conversion.");
        }

        output.Position = 0;
        return output;
    }

    private void ConvertWordStreamToPdf(Stream wordStream, Stream outPdfStream, string ext)
    {
        // Load Word document (DocIO)
        // FormatType.Automatic will deduce .doc / .docx / .rtf / .txt
        using var wordDoc = new WordDocument(wordStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();

        // Optionally configure settings (embedding fonts, bookmarks, etc.)
        // e.g. renderer.Settings.EmbedFonts = true;

        using var pdfDoc = renderer.ConvertToPDF(wordDoc);

        pdfDoc.Save(outPdfStream);
    }

    private void ConvertTextStreamToPdf(Stream textStream, Stream outPdfStream)
    {
        if (textStream.CanSeek)
            textStream.Position = 0;

        string content;
        using (var reader = new StreamReader(textStream))
        {
            content = reader.ReadToEnd();
        }

        using (var pdfDoc = new PdfDocument())
        {
            PdfPage page = pdfDoc.Pages.Add();
            var g = page.Graphics;

            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            float margin = 40f;

            var pageSize = page.GetClientSize();
            // Create a rectangle representing the drawing bounds
            var layoutBounds = new RectangleF(
                margin, margin,
                pageSize.Width - 2 * margin,
                pageSize.Height - 2 * margin
            );

            var element = new PdfTextElement(content, font, PdfBrushes.Black);
            element.StringFormat = new PdfStringFormat(PdfTextAlignment.Left);

            var layoutFormat = new PdfLayoutFormat
            {
                Break = PdfLayoutBreakType.FitPage,
                Layout = PdfLayoutType.Paginate
            };

            // Use the rectangle-based overload
            element.Draw(page, layoutBounds, layoutFormat);

            pdfDoc.Save(outPdfStream);
        }

        outPdfStream.Position = 0;
    }

    private void ConvertExcelStreamToPdf(Stream excelStream, Stream outPdfStream, string ext)
    {
        using var excelEngine = new ExcelEngine();
        var app = excelEngine.Excel;
        app.DefaultVersion = ExcelVersion.Xlsx;  // or Excel version you expect

        IWorkbook workbook = app!.Workbooks.Open(excelStream!);

        // Use the renderer to convert to PDF
        var renderer = new XlsIORenderer();

        PdfDocument pdfDoc = renderer.ConvertToPDF(workbook);

        pdfDoc.Save(outPdfStream);
    }

    private void ConvertImageStreamToPdf(Stream imgStream, Stream outPdfStream)
    {
        using var pdfDoc = new PdfDocument();
        PdfPage page = pdfDoc.Pages.Add();
        var g = page.Graphics;

        using var image = new PdfBitmap(imgStream);

        var clientSize = page.GetClientSize();
        float ratio = Math.Min(clientSize.Width / image.Width, clientSize.Height / image.Height);
        float w = image.Width * ratio;
        float h = image.Height * ratio;
        float x = (clientSize.Width - w) / 2;
        float y = (clientSize.Height - h) / 2;

        g.DrawImage(image, x, y, w, h);

        pdfDoc.Save(outPdfStream);
    }
}
