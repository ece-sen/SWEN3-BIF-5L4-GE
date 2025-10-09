using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Paperless.DAL;
using Paperless.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Paperless.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DMSController : ControllerBase
    {
        private readonly IDocumentRepository _repository;

        public DMSController(IDocumentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetAll()
        {
            var documents = await _repository.GetAllDocumentsAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetById(int id)
        {
            var doc = await _repository.GetDocumentByIdAsync(id);
            if (doc == null)
                return NotFound($"Document with ID {id} not found.");
            return Ok(doc);
        }

        [HttpPost]
        public async Task<ActionResult<Document>> Create([FromBody] Document newDoc)
        {
            var created = await _repository.AddDocumentAsync(newDoc);

            // 201 Created + Rückgabe-Link
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _repository.GetDocumentByIdAsync(id);
            if (doc == null)
                return NotFound();

            await _repository.DeleteDocumentAsync(id);
            return NoContent();
        }
    }
}
