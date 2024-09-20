namespace ProductosMongoRestNet.Config.Database;

public class AuthJwtConfig
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ExpiresInMinutes { get; set; } = string.Empty;
}