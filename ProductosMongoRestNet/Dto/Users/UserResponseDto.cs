namespace ProductosMongoRestNet.Dto.Users;

public class UserResponseDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; } // e.g., "Admin" or "User"
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}