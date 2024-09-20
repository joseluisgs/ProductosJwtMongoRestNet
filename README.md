# ProductosJwtMongoRestNet

Ejemplo de una API REST básica en .NET Core 8 con MongoDB y JWT para autenticación y autorización.
![image](./image/image.webp)

- [ProductosJwtMongoRestNet](#productosjwtmongorestnet)
  - [Descripción](#descripción)
  - [Endpoints](#endpoints)
  - [Librerías usadas](#librerías-usadas)


## Descripción

Este proyecto es un ejemplo de una API REST básica en .NET Core 8 con MongoDB con  [proyecto](https://github.com/joseluisgs/ProductosStorageMongoRestNet)

Cuidado con las configuraciones y la inyección de los servicios

Mongo esta en Mongo Atlas, por lo que la cadena de conexión es un poco diferente.


## Endpoints
- Books: contiene el CRUD de los libros (GET, POST, PUT, DELETE)

## Librerías usadas
- MongoDB.Driver
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.IdentityModel.JsonWebTokens
- BCrypt.Net-Next

