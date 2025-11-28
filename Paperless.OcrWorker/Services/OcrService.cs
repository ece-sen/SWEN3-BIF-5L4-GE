using System.Text;

namespace Paperless.OcrWorker.Services
{
    public class OcrService
    {
        private readonly IProcessRunner _process;
        private readonly IFileSystem _fs;

        public OcrService(IProcessRunner process, IFileSystem fs)
        {
            _process = process;
            _fs = fs;
        }

        public string ExtractTextFromPdf(string pdfPath)
        {
            string tempDir = Path.GetTempPath();
            string outputPattern = Path.Combine(tempDir, "page-%03d.png");

            _process.Run("gs",
                $"-dNOPAUSE -dBATCH -sDEVICE=png16m -r300 -sOutputFile={outputPattern} {pdfPath}");

            var pages = _fs.GetFiles(tempDir, "page-*.png").OrderBy(x => x).ToList();

            var result = new StringBuilder();

            foreach (var page in pages)
            {
                string outputBase = Path.Combine(tempDir, "ocr-temp");

                _process.Run("tesseract", $"{page} {outputBase} -l eng");

                string txtFile = outputBase + ".txt";

                if (_fs.Exists(txtFile))
                {
                    result.AppendLine(_fs.ReadAllText(txtFile));
                    _fs.Delete(txtFile);
                }

                _fs.Delete(page);
            }

            return result.ToString();
        }
    }
}
