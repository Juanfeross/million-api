using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using API.Models;

namespace API.Controllers;

/// <summary>
/// Controlador para gestionar propiedades
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PropertiesController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertiesController> _logger;

    /// <summary>
    /// Constructor que inicializa el controlador
    /// </summary>
    public PropertiesController(
        IPropertyService propertyService,
        ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una página paginada de todas las propiedades disponibles
    /// </summary>
    /// <param name="page">Número de página (mínimo 1, por defecto 1)</param>
    /// <param name="pageSize">Cantidad de elementos por página (mínimo 1, máximo 100, por defecto 20)</param>
    /// <returns>Respuesta con la página de propiedades solicitada, incluyendo información de paginación</returns>
    /// <response code="200">Retorna la página de propiedades exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Este endpoint devuelve una lista paginada de propiedades. Cada propiedad incluye:
    /// - Información básica (nombre, dirección, precio)
    /// - Imagen principal
    /// - Nombre del propietario
    ///
    /// Ejemplo de uso:
    /// ```
    /// GET /api/properties?page=1&amp;pageSize=20
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PropertyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<PropertyDto>>>> GetAllProperties(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        try
        {
            page = page <= 0 ? DefaultPage : page;
            pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var result = await _propertyService.GetAllPropertiesPagedAsync(page, pageSize);
            return Ok(ApiResponse<PagedResult<PropertyDto>>.SuccessResponse(result, "Propiedades obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las propiedades");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener las propiedades", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtiene los detalles completos de una propiedad específica por su identificador
    /// </summary>
    /// <param name="id">Identificador único de la propiedad (ObjectId de MongoDB)</param>
    /// <returns>Detalles completos de la propiedad incluyendo todas las imágenes y información del propietario</returns>
    /// <response code="200">Retorna los detalles de la propiedad exitosamente</response>
    /// <response code="404">La propiedad no fue encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Este endpoint devuelve información detallada de una propiedad, incluyendo:
    /// - Todos los datos de la propiedad
    /// - Lista completa de imágenes habilitadas
    /// - Información completa del propietario
    /// - Historial de trazas (PropertyTraces) ordenado por fecha
    ///
    /// Ejemplo de uso:
    /// ```
    /// GET /api/properties/507f1f77bcf86cd799439011
    /// ```
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PropertyDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PropertyDetailDto>>> GetPropertyById(string id)
    {
        try
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Propiedad no encontrada"));
            }

            return Ok(ApiResponse<PropertyDetailDto>.SuccessResponse(property, "Propiedad obtenida exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la propiedad {PropertyId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener la propiedad", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Busca propiedades aplicando filtros opcionales y devuelve resultados paginados
    /// </summary>
    /// <param name="name">Búsqueda parcial por nombre de la propiedad (case-insensitive)</param>
    /// <param name="address">Búsqueda parcial por dirección de la propiedad (case-insensitive)</param>
    /// <param name="minPrice">Precio mínimo de la propiedad (decimal)</param>
    /// <param name="maxPrice">Precio máximo de la propiedad (decimal)</param>
    /// <param name="page">Número de página (mínimo 1, por defecto 1)</param>
    /// <param name="pageSize">Cantidad de elementos por página (mínimo 1, máximo 100, por defecto 20)</param>
    /// <returns>Respuesta con la página de propiedades que coinciden con los filtros</returns>
    /// <response code="200">Retorna la página de propiedades filtradas exitosamente</response>
    /// <response code="400">Error en los parámetros de búsqueda</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Permite buscar propiedades usando uno o más filtros. Todos los filtros son opcionales y se combinan con AND.
    ///
    /// Ejemplos de uso:
    /// ```
    /// GET /api/properties/search?name=casa&amp;minPrice=100000&amp;maxPrice=500000
    /// GET /api/properties/search?address=Madrid&amp;page=1&amp;pageSize=10
    /// GET /api/properties/search?minPrice=200000
    /// ```
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PropertyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<PropertyDto>>>> SearchProperties(
        [FromQuery] string? name = null,
        [FromQuery] string? address = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        try
        {
            page = page <= 0 ? DefaultPage : page;
            pageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var filter = new PropertyFilterDto
            {
                Name = name,
                Address = address,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var result = await _propertyService.SearchPropertiesPagedAsync(filter, page, pageSize);
            return Ok(ApiResponse<PagedResult<PropertyDto>>.SuccessResponse(result, "Búsqueda realizada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar propiedades");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al buscar propiedades", new List<string> { ex.Message }));
        }
    }

}

