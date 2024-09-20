# ProductosJwtMongoRestNet

Ejemplo de una API REST básica en .NET Core 8 con MongoDB y JWT para autenticación y autorización.
![image](./image/image.webp)

- [ProductosJwtMongoRestNet](#productosjwtmongorestnet)
  - [Descripción](#descripción)
  - [Endpoints](#endpoints)
  - [Librerías usadas](#librerías-usadas)
  - [Como funciona](#como-funciona)
  - [Opción A: con políticas de autenticación y autorización (TAG V1.0)](#opción-a-con-políticas-de-autenticación-y-autorización-tag-v10)
  - [Opción B: con políticas de autenticación y autorización basado en un middleware para que no haya información sensible](#opción-b-con-políticas-de-autenticación-y-autorización-basado-en-un-middleware-para-que-no-haya-información-sensible)
  - [Aumentar la seguridad](#aumentar-la-seguridad)


## Descripción

Este proyecto es un ejemplo de una API REST básica en .NET Core 8 con MongoDB con  [proyecto](https://github.com/joseluisgs/ProductosStorageMongoRestNet)

Cuidado con las configuraciones y la inyección de los servicios

Mongo esta en Mongo Atlas, por lo que la cadena de conexión es un poco diferente.


## Endpoints
- Books: contiene el CRUD de los libros (GET, POST, PUT, DELETE)
- Users: Login y Register

## Librerías usadas
- MongoDB.Driver
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.IdentityModel.JsonWebTokens
- BCrypt.Net-Next

## Como funciona

El middleware de autorización en ASP.NET Core se encarga de autenticar y autorizar las solicitudes HTTP entrantes a tu aplicación. Aquí te explico cómo funciona y cómo extrae los datos de los tokens JWT.

- Autenticación; La autenticación verifica la identidad del usuario. En el caso de JWT, esta verificación se hace mediante el análisis y del token proporcionado en el encabezado de autorización de la solicitud HTTP.

- Autorización: La autorización determina si un usuario autenticado tiene permisos para realizar una acción específica. En ASP.NET Core, esto se hace evaluando los claims presentes en el token JWT contra las políticas y roles definidos en tu aplicación.

Por defecto haremos: `Petición -> Middleware de autenticación -> Middleware de autorización -> Controlador -> Respuesta`


## Opción A: con políticas de autenticación y autorización (TAG V1.0)

Lo primero es crear y configurar los servicios de JWT Authenticaction y de Authorization. Nuestro objetivo es conseguir:

`Petición -> Middleware de autenticación -> Middleware de autorización -> Controlador -> Respuesta`

```csharp
 // Configura la Autenticación de JWT
    var confiKey = configuration.GetSection("Jwt").Get<AuthJwtConfig>();
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(confiKey.Key));
    // Cargamos esta configuración para poder inyectarla porque la vamos a necesitar en el servicio
    myBuilder.Services.Configure<AuthJwtConfig>(
        myBuilder.Configuration.GetSection("Jwt"));
    // Configuramos la autenticación con JWT
    myBuilder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });

    // Configura la Autenticación con politicas de seguridad, en este caso admin
    myBuilder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    });
```

Para generar el token usamos
```csharp
public string GenerateJwtToken(Models.Users.User user)
    {
        _logger.LogInformation("Generating JWT token");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authJwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            // Aquí puedes añadir los claims que necesites
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, typeof(Role).GetEnumName(user.Role)) // El nombre del rol es el nombre del enum
        };

        var token = new JwtSecurityToken(
            _authJwtConfig.Issuer,
            _authJwtConfig.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_authJwtConfig.ExpiresInMinutes)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
```	

Para proteger las rutas usamos las anotaciones `[Authorize]` y `[Authorize(Policy = "AdminPolicy")]` dependiendo si solo queremos que estén autenticados y si además queremos que sean administradores (o cualquier otra cosa)
```csharp
Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    // Aquí irían tus endpoints de libros...
    
    [HttpGet]
    public IActionResult GetAllBooks() { /*...*/ }

    [HttpGet("{id}")]
    public IActionResult GetBookById(string id) { /*...*/ }

    [Authorize]
    [HttpPost]
    public IActionResult CreateBook([FromBody] Book book) { /*...*/ }

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult UpdateBook(string id, [FromBody] Book book) { /*...*/ }

    [Authorize(Policy = "AdminPolicy")]
    [HttpDelete("{id}")]
    public IActionResult DeleteBook(string id) { /*...*/ }
}
```

## Opción B: con políticas de autenticación y autorización basado en un middleware para que no haya información sensible

Vamos a quitar del token el rol, y crear un middleware que lo incluya el rol en el contexto de la aplicación. Este si situará entre el middleware de autenticación y el de autorización.
¿Qué es un middleware? Un middleware es un componente de software que se encuentra entre una aplicación cliente y el servidor. En este caso, el middleware se encuentra entre la solicitud HTTP y la aplicación ASP.NET Core. El objetivo de un middleware es procesar la solicitud entrante y, opcionalmente, modificar la respuesta saliente.

Nuestro objetivo es conseguir: `Petición -> Middleware de autenticación -> Middleware de Rol -> Middleware de autorización -> Controlador -> Respuesta`

El primer paso es quitar de la generación del token los datos sensibles, en este caso el rol
```csharp
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
```	

El siguiente paso es hacer el middleware que se encargue de añadir el rol al contexto de la aplicación usando el id del usuario o el username
```csharp
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
```

Obviamente el middleware debe ser añadido en el pipeline de la aplicación entre el middleware de autenticación y el de autorización
```csharp
// Habilitamos el middleware de Autorización y Autorización JWT
app.UseAuthentication();
// Middleware para gestionar los roles de los usuarios
app.UseMiddleware<RoleMiddleware>(); // Añadir middleware de roles, hay que meterlo en el pipeline antes de la autorización y después de la autenticación
app.UseAuthorization();
```	

## Aumentar la seguridad

- Uso de HTTPS: Sí, siempre se debe usar HTTPS para proteger la transmisión de datos sensibles, incluyendo tokens JWT. Esta es la medida de seguridad más básica y fundamental.

- Minimizar Información Sensible en el JWT: Si te preocupa que los usuarios puedan ver los roles y demás información en el token al decodificarlo, intenta mantener la información sensible fuera del JWT siempre que sea posible.
Usa un identificador de usuario en el JWT y realiza una consulta a la base de datos para obtener información sensible como roles, permisos, etc.

- Personalizar Verificación del Rol: Si prefieres no incluir roles directamente en el JWT, puedes usar un enfoque más dinámico para verificar los roles, como se describió anteriormente con la consulta a la base de datos en cada solicitud.

Ambas soluciones tienen sus méritos y la decisión depende de los requisitos específicos de tu aplicación:

- Uso de Claims en el JWT + HTTPS: 
  - Pros: Menos carga en el servidor, ya que no hay necesidad de hacer consultas adicionales a la base de datos en cada solicitud.
  - Contras: La información incluida en el JWT puede ser visible para el usuario si decodifica el token.

- Consulta Dinámica de Roles:
  -Pros: La información sensible (como roles) no está expuesta en el JWT. Permite una gestión de roles más dinámica y actualizada.
  -Contras: Introduce una carga adicional en el servidor debido a la necesidad de realizar consultas a la base de datos para cada solicitud autorizada.
