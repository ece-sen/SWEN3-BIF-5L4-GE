using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public class FileSystem : IFileSystem
    {
        public string[] GetFiles(string directory, string pattern)
            => Directory.GetFiles(directory, pattern);

        public string ReadAllText(string path)
            => File.ReadAllText(path);

        public void Delete(string path)
            => File.Delete(path);

        public bool Exists(string path)
            => File.Exists(path);
    }
}
