using Microsoft.AspNetCore.Mvc;
using Paperless.Models;

namespace Paperless.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DMSController : ControllerBase
    {
        private static readonly List<Document> Documents = new()
        {
            new Document { Id = 1, Title = "Invoice 2025-01", Category = "Finance" },
            new Document { Id = 2, Title = "Employee Contract", Category = "HR" },
            new Document { Id = 3, Title = "Product Manual", Category = "Tech" },
        };

        [HttpGet]
        public ActionResult<IEnumerable<Document>> GetAll()
        {
            return Ok(Documents);
        }

        [HttpGet("{id}")]
        public ActionResult<Document> GetById(int id)
        {
            var doc = Documents.FirstOrDefault(d => d.Id == id);
            if (doc == null)
                return NotFound($"Document with ID {id} not found.");
            return Ok(doc);
        }

        [HttpPost]
        public ActionResult<Document> Create([FromBody] Document newDoc)
        {
            newDoc.Id = Documents.Max(d => d.Id) + 1;
            Documents.Add(newDoc);

            // 201 Created + Rückgabe-Link
            return CreatedAtAction(nameof(GetById), new { id = newDoc.Id }, newDoc);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var doc = Documents.FirstOrDefault(d => d.Id == id);
            if (doc == null)
                return NotFound();

            Documents.Remove(doc);
            return NoContent();
        }
    }
}
