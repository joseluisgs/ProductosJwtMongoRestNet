using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Dto.Users;
using ProductosMongoRestNet.Models.Users;
using ProductosMongoRestNet.Services.User;

namespace ProductosMongoRestNet.Controllers.Users;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _userService;

    public UsersController(IUsersService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
    {
        // Vamos a exigir que el usuario no exista con el mismo nombre de usuario
        var existingUser = await _userService.GetUserByUsernameAsync(userDto.Username);
        if (existingUser != null) return BadRequest("User already exists");

        var user = new User
        {
            Username = userDto.Username,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(userDto
                    .Password), // El salt por defecto es 11, cuanto más alto, más seguro y más lento
            Role = Role.User // Default role
        };

        var savedUser = await _userService.CreateUserAsync(user);

        // Creamos la respuesta con el usuario creado
        var userResponseDto = new UserResponseDto
        {
            Id = savedUser.Id,
            Username = savedUser.Username,
            Role = typeof(Role).GetEnumName(savedUser.Role),
            CreatedAt = savedUser.CreatedAt,
            UpdatedAt = savedUser.UpdatedAt
        };
        return CreatedAtAction(nameof(Login), userResponseDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
    {
        // Vamos a exigir que el usuario exista y que la contraseña sea correcta
        var user = await _userService.GetUserByUsernameAsync(userDto.Username);
        // Si el usuario no existe o la contraseña no es correcta, devolvemos Unauthorized
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            return Unauthorized();

        var token = _userService.GenerateJwtToken(user);

        // Devolvemos el token en el response
        return Ok(new { token });
    }
}