using AutoMapper;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

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
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Constructor que inicializa el servicio de propiedades
    /// </summary>
    public PropertyService(
        IPropertyRepository propertyRepository,
        IOwnerRepository ownerRepository,
        IPropertyImageRepository propertyImageRepository,
        IMapper mapper,
        IMemoryCache cache)
    {
        _propertyRepository = propertyRepository;
        _ownerRepository = ownerRepository;
        _propertyImageRepository = propertyImageRepository;
        _mapper = mapper;
        _cache = cache;
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
        var cacheKey = $"properties_page_{page}_size_{pageSize}";

        if (_cache.TryGetValue(cacheKey, out PagedResult<PropertyDto>? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        var (items, total) = await _propertyRepository.GetPagedAsync(page, pageSize);
        var dtos = (await MapPropertiesToDtoAsync(items)).ToList();
        var result = new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<PagedResult<PropertyDto>> SearchPropertiesPagedAsync(PropertyFilterDto filter, int page, int pageSize)
    {
        var filterKey = $"name_{filter.Name}_addr_{filter.Address}_min_{filter.MinPrice}_max_{filter.MaxPrice}";
        var cacheKey = $"search_{filterKey}_page_{page}_size_{pageSize}";

        if (_cache.TryGetValue(cacheKey, out PagedResult<PropertyDto>? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        var (items, total) = await _propertyRepository.SearchPagedAsync(filter.Name, filter.Address, filter.MinPrice, filter.MaxPrice, page, pageSize);
        var dtos = (await MapPropertiesToDtoAsync(items)).ToList();
        var result = new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    /// <summary>
    /// Mapea una lista de propiedades a DTOs
    /// </summary>
    private async Task<IEnumerable<PropertyDto>> MapPropertiesToDtoAsync(IEnumerable<Property> properties)
    {
        var propertyList = properties.ToList();
        if (!propertyList.Any())
        {
            return new List<PropertyDto>();
        }

        var ownerIds = propertyList
            .Where(p => !string.IsNullOrEmpty(p.IdOwner))
            .Select(p => p.IdOwner!)
            .Distinct()
            .ToList();

        var propertyIds = propertyList
            .SelectMany(p => new[] { p.Id, p.IdProperty })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var ownersTask = GetOwnersCachedAsync(ownerIds);
        var imagesTask = GetFirstImagesCachedAsync(propertyIds);

        await Task.WhenAll(ownersTask, imagesTask);

        var ownersDict = await ownersTask;
        var imagesDict = await imagesTask;

        var dtos = new List<PropertyDto>();

        foreach (var property in propertyList)
        {
            var dto = _mapper.Map<PropertyDto>(property);

            if (!string.IsNullOrEmpty(property.IdOwner) && ownersDict.TryGetValue(property.IdOwner, out var owner))
            {
                dto.OwnerName = owner.Name;
            }

            if (imagesDict.TryGetValue(property.Id, out var image))
            {
                dto.Image = image.File;
            }
            else if (!string.IsNullOrEmpty(property.IdProperty) && imagesDict.TryGetValue(property.IdProperty, out var imageByPropertyId))
            {
                dto.Image = imageByPropertyId.File;
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

        var ownerIds = new List<string>();
        if (!string.IsNullOrEmpty(property.IdOwner))
        {
            ownerIds.Add(property.IdOwner);
        }

        var propertyIds = new List<string> { property.Id };
        if (!string.IsNullOrEmpty(property.IdProperty))
        {
            propertyIds.Add(property.IdProperty);
        }

        var ownersTask = GetOwnersCachedAsync(ownerIds);
        var imagesTask = GetEnabledImagesCachedAsync(propertyIds);

        await Task.WhenAll(ownersTask, imagesTask);

        var ownersDict = await ownersTask;
        var imagesDict = await imagesTask;

        if (!string.IsNullOrEmpty(property.IdOwner) && ownersDict.TryGetValue(property.IdOwner, out var owner))
        {
            dto.Owner = _mapper.Map<OwnerDto>(owner);
        }

        if (imagesDict.TryGetValue(property.Id, out var images))
        {
            dto.Images = images.Select(img => img.File).ToList();
        }
        else if (!string.IsNullOrEmpty(property.IdProperty) && imagesDict.TryGetValue(property.IdProperty, out var imagesByPropertyId))
        {
            dto.Images = imagesByPropertyId.Select(img => img.File).ToList();
        }
        else
        {
            dto.Images = new List<string>();
        }

        return dto;
    }

    private async Task<Dictionary<string, Owner>> GetOwnersCachedAsync(IEnumerable<string> ownerIds)
    {
        var ownerIdList = ownerIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (!ownerIdList.Any())
        {
            return new Dictionary<string, Owner>();
        }

        var result = new Dictionary<string, Owner>();
        var missingIds = new List<string>();

        foreach (var ownerId in ownerIdList)
        {
            var cacheKey = $"owner_{ownerId}";
            if (_cache.TryGetValue(cacheKey, out Owner? cachedOwner) && cachedOwner != null)
            {
                result[ownerId] = cachedOwner;
            }
            else
            {
                missingIds.Add(ownerId);
            }
        }

        if (missingIds.Any())
        {
            var loadedOwners = await _ownerRepository.GetOwnersByIdsAsync(missingIds);

            foreach (var owner in loadedOwners.Values)
            {
                foreach (var ownerId in missingIds)
                {
                    if (owner.Id == ownerId || owner.IdOwner == ownerId)
                    {
                        var cacheKey = $"owner_{ownerId}";
                        _cache.Set(cacheKey, owner, CacheExpiration);
                        if (!result.ContainsKey(ownerId))
                        {
                            result[ownerId] = owner;
                        }
                        break;
                    }
                }
            }
        }

        return result;
    }

    private async Task<Dictionary<string, PropertyImage>> GetFirstImagesCachedAsync(IEnumerable<string> propertyIds)
    {
        var propertyIdList = propertyIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (!propertyIdList.Any())
        {
            return new Dictionary<string, PropertyImage>();
        }

        var result = new Dictionary<string, PropertyImage>();
        var missingIds = new List<string>();

        foreach (var propertyId in propertyIdList)
        {
            var cacheKey = $"image_first_{propertyId}";
            if (_cache.TryGetValue(cacheKey, out PropertyImage? cachedImage) && cachedImage != null)
            {
                result[propertyId] = cachedImage;
            }
            else
            {
                missingIds.Add(propertyId);
            }
        }

        if (missingIds.Any())
        {
            var loadedImages = await _propertyImageRepository.GetFirstEnabledImagesByPropertyIdsAsync(missingIds);

            foreach (var kvp in loadedImages)
            {
                var cacheKey = $"image_first_{kvp.Key}";
                _cache.Set(cacheKey, kvp.Value, CacheExpiration);
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    private async Task<Dictionary<string, List<PropertyImage>>> GetEnabledImagesCachedAsync(IEnumerable<string> propertyIds)
    {
        var propertyIdList = propertyIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (!propertyIdList.Any())
        {
            return new Dictionary<string, List<PropertyImage>>();
        }

        var result = new Dictionary<string, List<PropertyImage>>();
        var missingIds = new List<string>();

        foreach (var propertyId in propertyIdList)
        {
            var cacheKey = $"images_all_{propertyId}";
            if (_cache.TryGetValue(cacheKey, out List<PropertyImage>? cachedImages) && cachedImages != null)
            {
                result[propertyId] = cachedImages;
            }
            else
            {
                missingIds.Add(propertyId);
            }
        }

        if (missingIds.Any())
        {
            var loadedImages = await _propertyImageRepository.GetEnabledImagesByPropertyIdsAsync(missingIds);

            foreach (var kvp in loadedImages)
            {
                var cacheKey = $"images_all_{kvp.Key}";
                _cache.Set(cacheKey, kvp.Value, CacheExpiration);
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }
}

