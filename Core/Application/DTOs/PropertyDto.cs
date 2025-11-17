namespace Core.Application.DTOs;

/// <summary>
/// DTO para representar una propiedad en listados
/// </summary>
public class PropertyDto
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
    /// URL o ruta de la imagen principal de la propiedad
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del propietario
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
}

