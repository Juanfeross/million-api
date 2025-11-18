namespace Core.Application.DTOs;

/// <summary>
/// DTO para filtros de búsqueda de propiedades
/// </summary>
public class PropertyFilterDto
{
    /// <summary>
    /// Nombre de la propiedad (búsqueda parcial)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Dirección de la propiedad (búsqueda parcial)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Precio mínimo
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Precio máximo
    /// </summary>
    public decimal? MaxPrice { get; set; }
}

