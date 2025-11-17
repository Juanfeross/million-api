using AutoMapper;
using Core.Application.DTOs;
using Core.Domain.Entities;

namespace Core.Application.Mappings;

/// <summary>
/// Perfil de mapeo para propiedades
/// </summary>
public class PropertyProfile : Profile
{
    /// <summary>
    /// Constructor que define los mapeos
    /// </summary>
    public PropertyProfile()
    {
        // Property -> PropertyDto
        CreateMap<Property, PropertyDto>()
            .ForMember(dest => dest.IdProperty, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IdOwner, opt => opt.MapFrom(src => src.IdOwner))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Image, opt => opt.Ignore()) // Se llena en el servicio
            .ForMember(dest => dest.OwnerName, opt => opt.Ignore()); // Se llena en el servicio

        // Property -> PropertyDetailDto
        CreateMap<Property, PropertyDetailDto>()
            .ForMember(dest => dest.IdProperty, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IdOwner, opt => opt.MapFrom(src => src.IdOwner))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.CodeInternal, opt => opt.MapFrom(src => src.CodeInternal))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year));

        // Owner -> OwnerDto
        CreateMap<Owner, OwnerDto>()
            .ForMember(dest => dest.IdOwner, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday));
    }
}

