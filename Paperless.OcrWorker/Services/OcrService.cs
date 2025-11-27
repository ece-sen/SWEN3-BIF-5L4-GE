using Ghostscript.NET.Rasterizer;
using Tesseract;
using System.Drawing;

namespace Paperless.OcrWorker.Services
{
    public class OcrService
    {
        public string ExtractTextFromPdf(string pdfPath)
        {
            var result = new System.Text.StringBuilder();

            using var rasterizer = new GhostscriptRasterizer();
            rasterizer.Open(pdfPath);

            using var engine = new TesseractEngine("/usr/share/tesseract-ocr/4.00/tessdata", "eng", EngineMode.Default);

            for (int page = 1; page <= rasterizer.PageCount; page++)
            {
                using Image img = rasterizer.GetPage(300, page);

                using var pix = Pix.LoadFromMemory(ImageToBytes(img));
                using var pageResult = engine.Process(pix);

                result.AppendLine(pageResult.GetText());
            }

            return result.ToString();
        }

        private byte[] ImageToBytes(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
