namespace Core.Application.DTOs;

/// <summary>
/// DTO para representar un propietario
/// </summary>
public class OwnerDto
{
    /// <summary>
    /// Identificador único del propietario
    /// </summary>
    public string IdOwner { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del propietario
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del propietario
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Foto del propietario
    /// </summary>
    public string Photo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de nacimiento del propietario
    /// </summary>
    public DateTime Birthday { get; set; }
}

