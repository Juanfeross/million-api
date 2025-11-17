using AutoMapper;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

namespace Core.Application.Services;

/// <summary>
/// Servicio de aplicación para propiedades
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPropertyImageRepository _propertyImageRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor que inicializa el servicio de propiedades
    /// </summary>
    public PropertyService(
        IPropertyRepository propertyRepository,
        IOwnerRepository ownerRepository,
        IPropertyImageRepository propertyImageRepository,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _ownerRepository = ownerRepository;
        _propertyImageRepository = propertyImageRepository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync()
    {
        var properties = await _propertyRepository.GetAllAsync();
        return await MapPropertiesToDtoAsync(properties);
    }

    /// <inheritdoc />
    public async Task<PropertyDetailDto?> GetPropertyByIdAsync(string id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null)
        {
            return null;
        }

        return await MapPropertyToDetailDtoAsync(property);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PropertyDto>> SearchPropertiesAsync(PropertyFilterDto filter)
    {
        var properties = await _propertyRepository.SearchAsync(
            filter.Name,
            filter.Address,
            filter.MinPrice,
            filter.MaxPrice
        );

        return await MapPropertiesToDtoAsync(properties);
    }

    /// <inheritdoc />
    public async Task<string?> GetPropertyImageAsync(string propertyId)
    {
        var image = await _propertyImageRepository.GetFirstEnabledImageByPropertyIdAsync(propertyId);
        return image?.File;
    }

    public async Task<PagedResult<PropertyDto>> GetAllPropertiesPagedAsync(int page, int pageSize)
    {
        var (items, total) = await _propertyRepository.GetPagedAsync(page, pageSize);
        var dtos = (await MapPropertiesToDtoAsync(items)).ToList();
        return new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<PropertyDto>> SearchPropertiesPagedAsync(PropertyFilterDto filter, int page, int pageSize)
    {
        var (items, total) = await _propertyRepository.SearchPagedAsync(filter.Name, filter.Address, filter.MinPrice, filter.MaxPrice, page, pageSize);
        var dtos = (await MapPropertiesToDtoAsync(items)).ToList();
        return new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Mapea una lista de propiedades a DTOs
    /// </summary>
    private async Task<IEnumerable<PropertyDto>> MapPropertiesToDtoAsync(IEnumerable<Property> properties)
    {
        var dtos = new List<PropertyDto>();

        foreach (var property in properties)
        {
            var dto = _mapper.Map<PropertyDto>(property);

            // Obtener información del propietario
            if (!string.IsNullOrEmpty(property.IdOwner))
            {
                // Buscar por IdOwner (que es un string que puede ser ObjectId o IdOwner)
                var owner = await _ownerRepository.GetByIdAsync(property.IdOwner);
                if (owner == null)
                {
                    // Si no se encuentra por Id, intentar buscar por IdOwner
                    var owners = await _ownerRepository.FindAsync(o => o.IdOwner == property.IdOwner);
                    owner = owners.FirstOrDefault();
                }
                
                if (owner != null)
                {
                    dto.OwnerName = owner.Name;
                }
            }

            // Obtener imagen principal (primera imagen habilitada)
            // Intentar buscar por Id de MongoDB primero, luego por IdProperty
            var image = await _propertyImageRepository.GetFirstEnabledImageByPropertyIdAsync(property.Id);
            if (image == null && !string.IsNullOrEmpty(property.IdProperty))
            {
                image = await _propertyImageRepository.GetFirstEnabledImageByPropertyIdAsync(property.IdProperty);
            }
            
            if (image != null)
            {
                dto.Image = image.File;
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    /// <summary>
    /// Mapea una propiedad a DTO detallado
    /// </summary>
    private async Task<PropertyDetailDto> MapPropertyToDetailDtoAsync(Property property)
    {
        var dto = _mapper.Map<PropertyDetailDto>(property);

        // Obtener información del propietario
        if (!string.IsNullOrEmpty(property.IdOwner))
        {
            // Buscar por IdOwner (que es un string que puede ser ObjectId o IdOwner)
            var owner = await _ownerRepository.GetByIdAsync(property.IdOwner);
            if (owner == null)
            {
                // Si no se encuentra por Id, intentar buscar por IdOwner
                var owners = await _ownerRepository.FindAsync(o => o.IdOwner == property.IdOwner);
                owner = owners.FirstOrDefault();
            }
            
            if (owner != null)
            {
                dto.Owner = _mapper.Map<OwnerDto>(owner);
            }
        }

        // Obtener todas las imágenes habilitadas
        // Intentar buscar por Id de MongoDB primero, luego por IdProperty
        var images = await _propertyImageRepository.GetEnabledImagesByPropertyIdAsync(property.Id);
        if (!images.Any() && !string.IsNullOrEmpty(property.IdProperty))
        {
            images = await _propertyImageRepository.GetEnabledImagesByPropertyIdAsync(property.IdProperty);
        }
        dto.Images = images.Select(img => img.File).ToList();

        return dto;
    }
}

