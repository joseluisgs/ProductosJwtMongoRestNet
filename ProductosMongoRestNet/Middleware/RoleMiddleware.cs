using System.Security.Claims;
using ProductosMongoRestNet.Models.Users;
using ProductosMongoRestNet.Services.User;

namespace ProductosMongoRestNet.Middleware;

/**
 * Middleware para gestionar los roles de los usuarios
 */
public class RoleMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly IUsersService _userService;

    public RoleMiddleware(RequestDelegate next, IUsersService userService, ILogger<RoleMiddleware> logger)
    {
        _next = next;
        _userService = userService;
        _logger = logger;
    }

    // Método para gestionar la petición
    public async Task InvokeAsync(HttpContext context)
    {
        // Si el usuario está autenticado
        if (context.User.Identity.IsAuthenticated)
        {
            _logger.LogDebug("User is authenticated");
            // Obtenemos los datos de los claims del token (Mira el método GenerateJwtToken en UsersService)
            var userId = context.User.FindFirst("UserId")?.Value;
            //var username = context.User.Identity.Name ?? string.Empty;

            // Buscamos el usuario por el id
            var user = await _userService.GetUserByIdAsync(userId);
            // Buscamos el usuario por el nombre de usuario
            //var user = await _userService.GetUserByUsernameAsync(username);

            // Si el usuario existe
            if (user != null)
            {
                _logger.LogDebug("User found, adding roles to claims");
                // Añadimos los roles del usuario a los claims
                var claims = new List<Claim>
                {
                    // Añadir claims con el role que es lo que necesitamos para el middleware de autorización
                    // new(ClaimTypes.Name, user.Username),
                    new("UserId", user.Id),
                    new(ClaimTypes.Role, typeof(Role).GetEnumName(user.Role)) // Añadir roles
                };

                // Creamos la identidad con los claims
                var identity = new ClaimsIdentity(claims, "custom");

                // Añadimos la identidad al usuario al contexto de la petición
                _logger.LogDebug("Adding identity to user");
                context.User.AddIdentity(identity);
            }
        }

        _logger.LogInformation("Invoking next middleware");
        // Pasamos la petición al siguiente middleware o controlador
        await _next(context);
    }
}