using AutoMapper;
using Paperless.DTOs;
using Paperless.Models;

namespace Paperless.Services.Mappings;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        // Entity -> DTO
        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.IsFavorite, opt => opt.MapFrom(src => src.Favorite != null))
            .ForMember(d => d.File, opt => opt.Ignore()); 

        // DTO -> Entity
        CreateMap<DocumentDto, Document>()
            .ForMember(d => d.Favorite, opt => opt.Ignore())  
            .ForMember(d => d.Id, opt => opt.Ignore());       

    }
}