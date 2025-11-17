namespace Core.Domain.Entities;

/// <summary>
/// Entidad que representa el historial de ventas de una propiedad
/// </summary>
public class PropertyTrace
{
    /// <summary>
    /// Identificador único del registro de trazabilidad
    /// </summary>
    public string IdPropertyTrace { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de venta
    /// </summary>
    public DateTime DateSale { get; set; }

    /// <summary>
    /// Nombre del registro
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Valor de la venta
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Impuesto de la venta
    /// </summary>
    public decimal Tax { get; set; }

    /// <summary>
    /// Identificador de la propiedad
    /// </summary>
    public string IdProperty { get; set; } = string.Empty;
}

