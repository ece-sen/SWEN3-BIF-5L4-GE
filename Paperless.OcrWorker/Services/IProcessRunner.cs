using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.OcrWorker.Services
{
    public interface IProcessRunner
    {
        int Run(string fileName, string arguments);
    }
}
