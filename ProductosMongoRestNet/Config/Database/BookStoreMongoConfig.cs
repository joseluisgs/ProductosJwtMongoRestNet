using MongoDB.Bson;
using MongoDB.Driver;

namespace ProductosMongoRestNet.Database;

public class BookStoreMongoConfig
{
    private readonly ILogger
        _logger; // Add ILogger to the class No hagas por constructor, porque es una clase de configuración

    public BookStoreMongoConfig(ILogger<BookStoreMongoConfig> logger)
    {
        _logger = logger;
    }

    // Esto es fundamental para que funcione la inyección de dependencias, al ser una clase de configuración
    // necesita un copnstructoir vacío, ya que el otro es para la inyección de dependencias
    public BookStoreMongoConfig()
    {
    }

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string BooksCollectionName { get; set; } = string.Empty;
    public string UsersCollectionName { get; set; } = string.Empty;
    
}