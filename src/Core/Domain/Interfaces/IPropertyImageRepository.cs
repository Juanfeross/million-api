using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de imágenes de propiedades
/// </summary>
public interface IPropertyImageRepository
{
    /// <summary>
    /// Obtiene todas las imágenes de una propiedad
    /// </summary>
    Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(string propertyId);

    /// <summary>
    /// Obtiene la primera imagen habilitada de una propiedad
    /// </summary>
    Task<PropertyImage?> GetFirstEnabledImageByPropertyIdAsync(string propertyId);

    /// <summary>
    /// Obtiene todas las imágenes habilitadas de una propiedad
    /// </summary>
    Task<IEnumerable<PropertyImage>> GetEnabledImagesByPropertyIdAsync(string propertyId);

    /// <summary>
    /// Obtiene la primera imagen habilitada de múltiples propiedades (batch loading)
    /// </summary>
    Task<Dictionary<string, PropertyImage>> GetFirstEnabledImagesByPropertyIdsAsync(IEnumerable<string> propertyIds);

    /// <summary>
    /// Obtiene todas las imágenes habilitadas de múltiples propiedades (batch loading)
    /// </summary>
    Task<Dictionary<string, List<PropertyImage>>> GetEnabledImagesByPropertyIdsAsync(IEnumerable<string> propertyIds);
}

