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
    /// Obtiene una página de propiedades
    /// </summary>
    /// <param name="page">Número de página (>=1)</param>
    /// <param name="pageSize">Tamaño de página (1-100)</param>
    /// <returns>Página de propiedades</returns>
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
    /// Obtiene una propiedad por su identificador
    /// </summary>
    /// <param name="id">Identificador de la propiedad</param>
    /// <returns>Detalles de la propiedad</returns>
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
    /// Busca propiedades según los filtros proporcionados
    /// </summary>
    /// <param name="name">Nombre de la propiedad (opcional)</param>
    /// <param name="address">Dirección de la propiedad (opcional)</param>
    /// <param name="minPrice">Precio mínimo (opcional)</param>
    /// <param name="maxPrice">Precio máximo (opcional)</param>
    /// <param name="page">Número de página (>=1)</param>
    /// <param name="pageSize">Tamaño de página (1-100)</param>
    /// <returns>Página de propiedades filtradas</returns>
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

    /// <summary>
    /// Obtiene la imagen principal de una propiedad
    /// </summary>
    /// <param name="id">Identificador de la propiedad</param>
    /// <returns>URL o ruta de la imagen</returns>
    [HttpGet("{id}/image")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> GetPropertyImage(string id)
    {
        try
        {
            var image = await _propertyService.GetPropertyImageAsync(id);
            if (string.IsNullOrEmpty(image))
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Imagen no encontrada"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(image, "Imagen obtenida exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la imagen de la propiedad {PropertyId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al obtener la imagen", new List<string> { ex.Message }));
        }
    }
}

