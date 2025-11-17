using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Domain.Entities;

/// <summary>
/// Entidad que representa un propietario de propiedades
/// </summary>
public class Owner
{
    /// <summary>
    /// Identificador único de MongoDB
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único del propietario
    /// </summary>
    [BsonElement("IdOwner")]
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
    /// Foto del propietario (URL o ruta)
    /// </summary>
    public string Photo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de nacimiento del propietario
    /// </summary>
    public DateTime Birthday { get; set; }
}

