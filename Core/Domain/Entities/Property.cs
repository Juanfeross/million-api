using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Domain.Entities;

/// <summary>
/// Entidad que representa una propiedad inmobiliaria
/// </summary>
public class Property
{
    /// <summary>
    /// Identificador único de MongoDB
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único de la propiedad
    /// </summary>
    [BsonElement("IdProperty")]
    public string IdProperty { get; set; } = string.Empty;

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
    /// Identificador del propietario
    /// </summary>
    public string IdOwner { get; set; } = string.Empty;
}

