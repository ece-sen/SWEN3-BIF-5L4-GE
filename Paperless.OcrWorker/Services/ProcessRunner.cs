using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Paperless.OcrWorker.Services
{
    public class ProcessRunner : IProcessRunner
    {
        public int Run(string fileName, string arguments)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });

            p?.WaitForExit();
            return p?.ExitCode ?? -1;
        }
    }
}
