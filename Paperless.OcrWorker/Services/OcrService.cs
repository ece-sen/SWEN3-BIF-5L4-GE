using System.Diagnostics;

namespace Paperless.OcrWorker.Services
{
    public class OcrService
    {
        public string ExtractTextFromPdf(string pdfPath)
        {
            // 1) Convert PDF → PNG pages using Ghostscript CLI
            string outputPattern = Path.Combine(Path.GetTempPath(), "page-%03d.png");

            var gs = Process.Start(new ProcessStartInfo
            {
                FileName = "gs",
                Arguments = $"-dNOPAUSE -dBATCH -sDEVICE=png16m -r300 -sOutputFile={outputPattern} {pdfPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            gs?.WaitForExit();

            // 2) OCR each PNG with Tesseract
            var result = new System.Text.StringBuilder();

            var pages = Directory.GetFiles(Path.GetTempPath(), "page-*.png")
                .OrderBy(f => f);

            foreach (var page in pages)
            {
                var outputBase = Path.Combine(Path.GetTempPath(), "ocr-temp");

                var tess = Process.Start(new ProcessStartInfo
                {
                    FileName = "tesseract",
                    Arguments = $"{page} {outputBase} -l eng",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                tess?.WaitForExit();

                var txtFile = outputBase + ".txt";
                if (File.Exists(txtFile))
                {
                    result.AppendLine(File.ReadAllText(txtFile));
                    File.Delete(txtFile);
                }

                File.Delete(page);
            }

            return result.ToString();
        }
    }
}