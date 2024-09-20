namespace ProductosMongoRestNet.Services.User;

public interface IUsersService
{
    Task<Models.Users.User?> GetUserByUsernameAsync(string username);
    Task<Models.Users.User?> GetUserByIdAsync(string id);
    Task<Models.Users.User> CreateUserAsync(Models.Users.User user);

    string GenerateJwtToken(Models.Users.User user);
}