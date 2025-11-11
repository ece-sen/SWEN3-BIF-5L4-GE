using Microsoft.AspNetCore.Mvc;
using Paperless.DAL.Exceptions;
using Paperless.DTOs;
using Paperless.Models;
using Paperless.Services;
using Paperless.Services.Exceptions;

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
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (DatabaseOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetById(int id)
        {
            try
            {
                var doc = await _documentService.GetDocumentByIdAsync(id);
                if (doc == null)
                    return NotFound(new { message = $"Document with ID {id} not found." });
                return Ok(doc);
            }
            catch (DocumentNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> Create([FromBody] DocumentDto newDoc)
        {
            try
            {
                var created = await _documentService.CreateDocumentAsync(newDoc);

                // 201 Created + Rückgabe-Link
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (DocumentValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _documentService.DeleteDocumentAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Document with ID {id} not found." });

                return NoContent();
            }
            catch (DocumentNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }
    }
}
