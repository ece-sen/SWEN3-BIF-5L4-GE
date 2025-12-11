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
        private readonly IConfiguration _config;

        public DMSController(IDocumentService documentService, ILogger<DMSController> logger, IConfiguration config)
        {
            _documentService = documentService;
            _logger = logger;
            _config = config;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DocumentDto dto)
        {
            _logger.LogInformation("PUT /api/DMS/{Id} called", id);

            try
            {
                var updated = await _documentService.UpdateDocumentAsync(id, dto);
                _logger.LogInformation("Document {Id} updated successfully", id);

                return Ok(updated);
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogWarning(ex, "Document {Id} not found while updating", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DocumentValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating document {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while updating {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating document {Id}", id);
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] DocumentDto dto)
        {
            if (dto.File == null)
                return BadRequest(new { message = "File is required" });

            using var stream = dto.File.OpenReadStream();

            try
            {
                var createdDocument = await _documentService.CreateDocumentAsync(
                    dto,
                    stream,
                    dto.File.FileName
                );

                return CreatedAtAction(nameof(GetById),
                    new { id = createdDocument.Id },
                    createdDocument);
            }
            catch (DocumentValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DocumentServiceException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }
        }

        [HttpPatch("{id}/summary")]
        public async Task<IActionResult> UpdateSummary(int id, [FromBody] string summary)
        {
            _logger.LogInformation("PATCH /api/DMS/{Id}/summary called", id);
            try
            {
                var updated = await _documentService.UpdateSummaryAsync(id, summary);
                if (!updated)
                {
                    _logger.LogWarning("Update summary failed: Document {Id} not found", id);
                    return NotFound(new { message = $"Document with ID {id} not found." });
                }

                _logger.LogInformation("Document {Id} summary updated successfully", id);
                return NoContent();
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogWarning(ex, "Document {Id} not found while updating summary", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database operation failed while updating summary for document {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating summary for document {Id}", id);
                return StatusCode(500, new { message = "Unexpected error: " + ex.Message });
            }

        }
    }
}
