using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Domain.Entities;

/// <summary>
/// Entidad que representa una imagen de una propiedad
/// </summary>
public class PropertyImage
{
    /// <summary>
    /// Identificador único de MongoDB
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único de la imagen
    /// </summary>
    [BsonElement("IdPropertyImage")]
    public string IdPropertyImage { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la propiedad a la que pertenece la imagen
    /// </summary>
    [BsonElement("IdProperty")]
    public string IdProperty { get; set; } = string.Empty;

    /// <summary>
    /// Archivo de la imagen (URL o ruta)
    /// </summary>
    [BsonElement("file")]
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la imagen está habilitada
    /// </summary>
    [BsonElement("Enabled")]
    public bool Enabled { get; set; } = true;
}
