using AutoMapper;
using Paperless.DTOs;
using Paperless.Models;

namespace Paperless.Services.Mappings;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<Document, DocumentDto>();      
        CreateMap<DocumentDto, Document>();      
    }
}