namespace Core.Application.DTOs;

/// <summary>
/// DTO para representar los detalles completos de una propiedad
/// </summary>
public class PropertyDetailDto
{
    /// <summary>
    /// Identificador único de la propiedad
    /// </summary>
    public string IdProperty { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del propietario
    /// </summary>
    public string IdOwner { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la propiedad
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de la propiedad
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Precio de la propiedad
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Código interno de la propiedad
    /// </summary>
    public string CodeInternal { get; set; } = string.Empty;

    /// <summary>
    /// Año de construcción de la propiedad
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Lista de imágenes de la propiedad
    /// </summary>
    public List<string> Images { get; set; } = new();

    /// <summary>
    /// Información del propietario
    /// </summary>
    public OwnerDto? Owner { get; set; }
}

