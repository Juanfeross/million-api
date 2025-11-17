namespace API.Models;

/// <summary>
/// Modelo de respuesta de error
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código de estado HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Mensaje de error
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalles del error
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Lista de errores de validación
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp del error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

