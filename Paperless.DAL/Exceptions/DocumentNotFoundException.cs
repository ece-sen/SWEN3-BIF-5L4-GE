using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.DAL.Exceptions
{
    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(int id) 
            : base($"Document with ID {id} was not found in the database.") { }
    }
}
