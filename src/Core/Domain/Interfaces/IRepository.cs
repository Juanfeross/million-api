using System.Linq.Expressions;

namespace Core.Domain.Interfaces;

/// <summary>
/// Interfaz genérica para repositorios
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Obtiene una entidad por su identificador
    /// </summary>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Busca entidades según un filtro
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// Crea una nueva entidad
    /// </summary>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    Task<T> UpdateAsync(string id, T entity);

    /// <summary>
    /// Elimina una entidad por su identificador
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Verifica si existe una entidad con el identificador dado
    /// </summary>
    Task<bool> ExistsAsync(string id);
}

