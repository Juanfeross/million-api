namespace Core.Application.DTOs;

/// <summary>
/// Resultado paginado genérico
/// </summary>
/// <typeparam name="T">Tipo de elemento</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalPages => PageSize <= 0 ? 0 : (long)Math.Ceiling((double)Total / PageSize);
}
