using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de propiedades
/// </summary>
public interface IPropertyRepository : IRepository<Property>
{
    /// <summary>
    /// Busca propiedades por nombre
    /// </summary>
    Task<IEnumerable<Property>> FindByNameAsync(string name);

    /// <summary>
    /// Busca propiedades por dirección
    /// </summary>
    Task<IEnumerable<Property>> FindByAddressAsync(string address);

    /// <summary>
    /// Busca propiedades por rango de precio
    /// </summary>
    Task<IEnumerable<Property>> FindByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Busca propiedades con múltiples filtros
    /// </summary>
    Task<IEnumerable<Property>> SearchAsync(string? name, string? address, decimal? minPrice, decimal? maxPrice);

    /// <summary>
    /// Obtiene propiedades por propietario
    /// </summary>
    Task<IEnumerable<Property>> FindByOwnerAsync(string ownerId);

    /// <summary>
    /// Obtiene una página de propiedades
    /// </summary>
    Task<(IEnumerable<Property> Items, long Total)> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Busca propiedades paginadas por filtros
    /// </summary>
    Task<(IEnumerable<Property> Items, long Total)> SearchPagedAsync(string? name, string? address, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
}

