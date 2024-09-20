using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductosMongoRestNet.Database;

namespace ProductosMongoRestNet.Services.User;

public class UsersService : IUsersService
{
    private const string CacheKeyPrefix = "User_"; //Para evitar colisiones en la caché de memoria con otros elementos
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IMongoCollection<Models.User> _usersCollection; // o Modelo O Documento de MongoDB

    public UsersService(IOptions<BookStoreMongoConfig> bookStoreDatabaseSettings, ILogger<UsersService> logger,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
        _usersCollection =
            mongoDatabase.GetCollection<Models.User>(bookStoreDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<Models.User> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }

    public async Task<Models.User> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<Models.User> CreateUserAsync(Models.User user)
    {
        throw new NotImplementedException();
    }
}