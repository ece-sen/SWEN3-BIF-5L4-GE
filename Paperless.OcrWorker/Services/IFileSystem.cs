using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public interface IFileSystem
    {
        string[] GetFiles(string directory, string pattern);
        string ReadAllText(string path);
        void Delete(string path);
        bool Exists(string path);
    }
}
