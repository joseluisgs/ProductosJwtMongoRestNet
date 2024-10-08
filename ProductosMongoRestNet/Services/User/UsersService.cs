﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductosMongoRestNet.Config.Database;

namespace ProductosMongoRestNet.Services.User;

public class UsersService : IUsersService
{
    private const string
        CacheKeyPrefixUsername = "UserName_"; //Para evitar colisiones en la caché de memoria con otros elementos

    private const string
        CacheKeyPrefixId = "UserId_"; //Para evitar colisiones en la caché de memoria con otros elementos

    private readonly AuthJwtConfig _authJwtConfig;

    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IMongoCollection<Models.Users.User> _usersCollection; // o Modelo O Documento de MongoDB

    public UsersService(IOptions<BookStoreMongoConfig> bookStoreDatabaseSettings,
        IOptions<AuthJwtConfig> jwtConfig,
        ILogger<UsersService> logger,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
        _usersCollection =
            mongoDatabase.GetCollection<Models.Users.User>(bookStoreDatabaseSettings.Value.UsersCollectionName);
        _authJwtConfig = jwtConfig.Value;
    }

    public async Task<Models.Users.User?> GetUserByUsernameAsync(string username)
    {
        _logger.LogInformation($"Getting user with username: {username}");
        var cacheKey = CacheKeyPrefixUsername + username;
        // Primero intentamos obtener el usuario de la caché
        if (_memoryCache.TryGetValue(cacheKey, out Models.Users.User? cachedUser))
        {
            _logger.LogInformation("Getting user from cache");
            return cachedUser;
        }

        // Si no está en la caché, lo obtenemos de la base de datos
        _logger.LogInformation("Getting user from database");
        var user = await _usersCollection.Find(user => user.Username == username).FirstOrDefaultAsync();
        // Si el usuario está en la base de datos, lo guardamos en la caché
        if (user != null)
        {
            _logger.LogInformation("User not found in cache, caching it");
            _memoryCache.Set(cacheKey, user,
                TimeSpan.FromMinutes(30)); // Ajusta el tiempo de caché según tus necesidades
            _logger.LogInformation("Caching the user");
        }

        // Devolvemos el usuario
        return user;
    }

    public async Task<Models.Users.User?> GetUserByIdAsync(string id)
    {
        _logger.LogInformation($"Getting user with id: {id}");
        var cacheKey = CacheKeyPrefixId + id;
        // Primero intentamos obtener el usuario de la caché
        if (_memoryCache.TryGetValue(cacheKey, out Models.Users.User? cachedUser))
        {
            _logger.LogInformation("Getting user from cache");
            return cachedUser;
        }

        // Si no está en la caché, lo obtenemos de la base de datos
        _logger.LogInformation("Getting user from database");
        var user = await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        // Si el usuario está en la base de datos, lo guardamos en la caché
        if (user != null)
        {
            _logger.LogInformation("User not found in cache, caching it");
            _memoryCache.Set(cacheKey, user,
                TimeSpan.FromMinutes(30)); // Ajusta el tiempo de caché según tus necesidades
            _logger.LogInformation("Caching the user");
        }

        // Devolvemos el usuario
        return user;
    }

    public async Task<Models.Users.User> CreateUserAsync(Models.Users.User user)
    {
        _logger.LogInformation("Creating user");
        user.Id = ObjectId.GenerateNewId().ToString();
        var timeStamp = DateTime.Now;
        user.CreatedAt = timeStamp;
        user.UpdatedAt = timeStamp;

        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public string GenerateJwtToken(Models.Users.User user)
    {
        _logger.LogInformation("Generating JWT token");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authJwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            // Aquí puedes añadir los claims que necesites
            // new Claim(ClaimTypes.Name, user.Username),
            // Vamos a evitar un dato sensible como el rol, y solo vamos a enviar el id o el username
            //new Claim(ClaimTypes.Role, typeof(Role).GetEnumName(user.Role)) // El nombre del rol es el nombre del enum
            new Claim("UserId", user.Id) // Más aseptico al enviar el id del usuario
        };

        var token = new JwtSecurityToken(
            _authJwtConfig.Issuer,
            _authJwtConfig.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_authJwtConfig.ExpiresInMinutes)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}