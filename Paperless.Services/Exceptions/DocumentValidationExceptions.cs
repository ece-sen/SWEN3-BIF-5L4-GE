using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Services.Exceptions
{
    public class DocumentValidationException : Exception
    {
        public DocumentValidationException(string message)
            : base(message) { }
    }
}
