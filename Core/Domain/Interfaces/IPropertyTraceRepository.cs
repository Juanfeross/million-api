using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de trazabilidad de propiedades
/// </summary>
public interface IPropertyTraceRepository
{
    /// <summary>
    /// Obtiene todas las trazas de una propiedad por su ID
    /// </summary>
    Task<IEnumerable<PropertyTrace>> GetByPropertyIdAsync(string propertyId);

    /// <summary>
    /// Obtiene trazas de múltiples propiedades por sus IDs (batch loading)
    /// </summary>
    Task<Dictionary<string, List<PropertyTrace>>> GetTracesByPropertyIdsAsync(IEnumerable<string> propertyIds);
}

