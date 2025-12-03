using Microsoft.AspNetCore.Http;

namespace Paperless.DTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IFormFile? File { get; set; }

}