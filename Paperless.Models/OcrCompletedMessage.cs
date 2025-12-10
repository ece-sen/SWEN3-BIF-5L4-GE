using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Models
{
    public class OcrCompletedMessage
    {
        public int DocumentId { get; set; }
        public string Text { get; set; } = "";
    }
}

