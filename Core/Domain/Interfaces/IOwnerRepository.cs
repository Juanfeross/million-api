using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de propietarios
/// </summary>
public interface IOwnerRepository : IRepository<Owner>
{
    /// <summary>
    /// Busca propietarios por nombre
    /// </summary>
    Task<IEnumerable<Owner>> FindByNameAsync(string name);

    /// <summary>
    /// Obtiene múltiples propietarios por sus IDs (batch loading)
    /// </summary>
    Task<Dictionary<string, Owner>> GetOwnersByIdsAsync(IEnumerable<string> ownerIds);
}

