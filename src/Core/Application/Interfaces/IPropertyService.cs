using Core.Application.DTOs;

namespace Core.Application.Interfaces;

/// <summary>
/// Interfaz para el servicio de propiedades
/// </summary>
public interface IPropertyService
{
    /// <summary>
    /// Obtiene todas las propiedades
    /// </summary>
    Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync();

    /// <summary>
    /// Obtiene una propiedad por su identificador
    /// </summary>
    Task<PropertyDetailDto?> GetPropertyByIdAsync(string id);

    /// <summary>
    /// Busca propiedades según los filtros proporcionados
    /// </summary>
    Task<IEnumerable<PropertyDto>> SearchPropertiesAsync(PropertyFilterDto filter);

    /// <summary>
    /// Obtiene la primera imagen habilitada de una propiedad
    /// </summary>
    Task<string?> GetPropertyImageAsync(string propertyId);

    /// <summary>
    /// Obtiene una página de propiedades
    /// </summary>
    Task<PagedResult<PropertyDto>> GetAllPropertiesPagedAsync(int page, int pageSize);

    /// <summary>
    /// Busca propiedades paginadas según filtros
    /// </summary>
    Task<PagedResult<PropertyDto>> SearchPropertiesPagedAsync(PropertyFilterDto filter, int page, int pageSize);
}

