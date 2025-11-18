using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Models;

namespace API.Filters;

/// <summary>
/// Filtro global para manejo de excepciones
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    /// <summary>
    /// Constructor que inicializa el filtro
    /// </summary>
    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Maneja las excepciones que ocurren en la ejecución
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Error no manejado: {Message}", context.Exception.Message);

        var errorResponse = new ErrorResponse
        {
            StatusCode = 500,
            Message = "Error interno del servidor",
            Details = context.Exception.Message,
            Timestamp = DateTime.UtcNow
        };

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}

