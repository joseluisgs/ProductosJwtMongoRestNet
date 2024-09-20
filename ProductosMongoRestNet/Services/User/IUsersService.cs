namespace ProductosMongoRestNet.Services.User;

public interface IUsersService
{
    Task<Models.User> GetUserByUsernameAsync(string username);
    Task<Models.User> GetUserByIdAsync(string id);
    Task<Models.User> CreateUserAsync(Models.User user);
}