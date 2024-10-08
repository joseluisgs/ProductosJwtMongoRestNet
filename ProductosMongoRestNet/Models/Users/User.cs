﻿using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductosMongoRestNet.Models.Users;

public class User
{
    [BsonId] // Esto indica que el campo es el identificador de la colección
    [BsonRepresentation(BsonType.ObjectId)] // Esto indica que el campo es de tipo ObjectId
    public string Id { get; set; }

    public string Username { get; set; }

    // Se ignora en el JSON de salida para que no se muestre la contraseña en la lista de usuarios
    public string PasswordHash { get; set; }

    public Role Role { get; set; } // e.g., "Admin" or "User"

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonIgnoreIfNull]
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonIgnoreIfNull]
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}