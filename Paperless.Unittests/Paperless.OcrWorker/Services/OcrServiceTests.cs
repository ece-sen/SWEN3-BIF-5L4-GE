using NUnit.Framework;
using Paperless.OcrWorker.Services;

namespace Paperless.Unittests.Paperless.OcrWorker.Services
{
    public class OcrServiceTests
    {
        [Test]
        public void ExtractTextFromPdf_ShouldContainHelloWorld()
        {
            // Arrange
            var service = new OcrService();

            var pdfPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "Paperless.OcrWorker",
                "Services",
                "TestFiles",
                "test.pdf"
            );

            // Act
            var text = service.ExtractTextFromPdf(pdfPath);

            // Assert
            Assert.IsNotNull(text, "OCR result should not be null");
            Assert.IsTrue(text.Trim().Length > 0, "OCR result should not be empty");


            Assert.IsTrue(
                text.Contains("Hello World", StringComparison.OrdinalIgnoreCase),
                $"OCR output does not contain expected text. Output was:\n{text}"
            );
        }
    }
}
