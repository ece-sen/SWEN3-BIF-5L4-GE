using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Services
{
    public interface IElasticSearchClientWrapper
    {
        Task<List<string>> SearchIdsAsync(string index, string field, string query, CancellationToken ct = default);
    }
}
