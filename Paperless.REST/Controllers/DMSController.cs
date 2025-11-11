using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DMSController> _logger;

        public DMSController(IDocumentService documentService, ILogger<DMSController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
        {
            _logger.LogInformation("GET /api/DMS called");
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                _logger.LogInformation("Fetched {Count} documents", documents.Count);
                return Ok(documents);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while fetching documents");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching documents");
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetById(int id)
        {
            _logger.LogInformation("GET /api/DMS/{Id} called", id);
            try
            {
                var doc = await _documentService.GetDocumentByIdAsync(id);
                if (doc == null)
                {
                    _logger.LogWarning("Document with ID {Id} not found", id);
                    return NotFound(new { message = $"Document with ID {id} not found." });
                }
                _logger.LogInformation("Document with ID {Id} returned successfully", id);
                return Ok(doc);
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogWarning(ex, "Document with ID {Id} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while fetching document {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching document {Id}", id);
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> Create([FromBody] DocumentDto newDoc)
        {
            _logger.LogInformation("POST /api/DMS - Creating document with title '{Title}'", newDoc.Title);
            try
            {
                var created = await _documentService.CreateDocumentAsync(newDoc);

                // 201 Created + Rückgabe-Link
                _logger.LogInformation("Document created successfully with ID {Id}", created.Id);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (DocumentValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed for new document: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while creating document '{Title}'", newDoc.Title);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating document '{Title}'", newDoc.Title);
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("DELETE /api/DMS/{Id} called", id);
            try
            {
                var deleted = await _documentService.DeleteDocumentAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Delete failed: Document {Id} not found", id);
                    return NotFound(new { message = $"Document with ID {id} not found." });
                }

                _logger.LogInformation("Document {Id} deleted successfully", id);
                return NoContent();
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogWarning(ex, "Document {Id} not found while deleting", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while deleting document {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting document {Id}", id);
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }
    }
}
