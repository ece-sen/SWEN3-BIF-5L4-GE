using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Services.Exceptions
{
    public class DocumentServiceException : Exception
    {
        public DocumentServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
