using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Services
{
    public interface IElasticsearchSearchService
    {
        Task<List<string>> SearchDocumentIdsAsync(string query);

    }
}
