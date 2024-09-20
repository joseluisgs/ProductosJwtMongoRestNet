using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Models.Books;
using ProductosMongoRestNet.Services.Books;
using ProductosMongoRestNet.Services.Storage;

namespace ProductosMongoRestNet.Controllers.Books;

[ApiController]
[Route("api/[controller]")]
//[Route("api/books")]
public class BooksController : ControllerBase
{
    private const string _route = "api/storage";
    private readonly IBooksService _booksService;
    private readonly IFileStorageService _storageService;

    public BooksController(IBooksService booksService, IFileStorageService storageService)
    {
        _booksService = booksService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> GetAll()
    {
        var books = await _booksService.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id:length(24)}")] // Para que el id tenga 24 caracteres (ObjectId)
    public async Task<ActionResult<Book>> GetById(string id)
    {
        var book = await _booksService.GetByIdAsync(id);

        if (book is null) return NotFound("Book not found with the provided id: " + id);

        return book;
    }

    [HttpPost]
    [Authorize] // Solo los usuarios autenticados pueden crear libros
    public async Task<ActionResult<Book>> Create(Book book)
    {
        var savedBook = await _booksService.CreateAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, savedBook);
    }

    [HttpPut("{id:length(24)}")]
    [Authorize] // Solo los usuarios autenticados pueden actualizar libros
    public async Task<ActionResult> Update(
        string id,
        [FromBody] Book book)
    {
        var updatedBook = await _booksService.UpdateAsync(id, book);

        if (updatedBook is null) return NotFound("Book not found with the provided id: " + id);

        return Ok(updatedBook);
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Policy = "AdminPolicy")] // Solo los usuarios con el rol "Admin" pueden eliminar libros
    public async Task<ActionResult> Delete(string id)
    {
        var deletedBook = await _booksService.DeleteAsync(id);

        if (deletedBook is null) return NotFound("Book not found with the provided id: " + id);

        // Eliminamos la imagen
        try
        {
            if (!string.IsNullOrEmpty(deletedBook.Image))
            {
                // Debemos quitar la ruta del http y queda el nombre del fichero si lo guardamos así
                // Esto es el parche por almacenarla con la URL completa, cosa que no es recomendable
                if (deletedBook.Image.Contains(_route))
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{_route}/";
                    var fileName = deletedBook.Image.Replace(baseUrl, "");
                    await _storageService.DeleteFileAsync(fileName);
                }
                else
                {
                    // Eliminamos el fichero, de la manera normal
                    await _storageService.DeleteFileAsync(deletedBook.Image);
                }
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [RequestSizeLimit(10_000_000)] // Limitamos el tamaño del fichero a 10MB
    [Consumes("multipart/form-data")] // Necesitamos especificar que el tipo de contenido es multipart/form-data para subir el fichero
    [HttpPut("image/{id:length(24)}")]
    [Authorize(Policy = "AdminPolicy")] // Solo los usuarios admin pueden actualizar la imagen del libro
    public async Task<ActionResult> UpdateImage(
        string id,
        [FromForm] IFormFile file)
    {
        // Comprobamos que el fichero no sea nulo
        if (file == null || file.Length == 0)
            return BadRequest("Not file in the request");

        // Obtenemos el libro
        var book = await _booksService.GetByIdAsync(id);

        // Si el libro no existe, devolvemos un error
        if (book is null) return NotFound("Book not found with the provided id: " + id);
        try
        {
            // Guardamos el fichero
            var fileName = await _storageService.SaveFileAsync(file);

            // Actualizamos la URL de la imagen
            // Aquí es cuando debemos decidir si la queremos el no,bre del fichero o la URL
            // Mira el controlador de Storage para ver cómo se hace, yo lo he hecho con el nombre del fichero
            // así siempre lo puedes contruir con la URL base, desde el cliente.
            // Obtener la URL base
            // OJO QUE S NI NO NO LA VAS A PODER BORRAR; MIRA EL PARCHE DE DELETE EN EL SERVICIO DE STORAGE
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = $"{baseUrl}/{_route}/{fileName}";
            book.Image = fileUrl;
            //book.Image = fileName;
            return Ok(await _booksService.UpdateAsync(id, book));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}