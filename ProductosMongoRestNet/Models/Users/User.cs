namespace ProductosMongoRestNet.Models;

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } // e.g., "Admin" or "User"
}