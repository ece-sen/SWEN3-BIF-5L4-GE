using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public interface IStorageService
    {
        Task DownloadFileAsync(string bucket, string objectName, string destinationPath);
        Task UploadTextAsync(string bucket, string objectName, string content);
    }
}
