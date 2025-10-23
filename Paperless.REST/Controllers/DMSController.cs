using Microsoft.AspNetCore.Mvc;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services;

namespace Paperless.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DMSController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DMSController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetById(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null)
                return NotFound($"Document with ID {id} not found.");
            return Ok(doc);
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> Create([FromBody] DocumentDto newDoc)
        {
            var created = await _documentService.CreateDocumentAsync(newDoc);

            // 201 Created + Rückgabe-Link
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _documentService.DeleteDocumentAsync(id);
            if (!doc)
                return NotFound();
            
            return NoContent();
        }
    }
}
