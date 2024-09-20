namespace ProductosMongoRestNet.Config.Database;

public class BookStoreMongoConfig
{
    // Esto es fundamental para que funcione la inyección de dependencias, al ser una clase de configuración
    // necesita un copnstructoir vacío, ya que el otro es para la inyección de dependencias
    /*public BookStoreMongoConfig()
    {
    }*/

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string BooksCollectionName { get; set; } = string.Empty;
    public string UsersCollectionName { get; set; } = string.Empty;
}