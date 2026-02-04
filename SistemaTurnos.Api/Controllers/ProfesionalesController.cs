using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador, Profesional, Cliente, Secretario")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalService _service;
    private readonly IWebHostEnvironment _environment;

    public ProfesionalesController(IProfesionalService service, IWebHostEnvironment environment)
    {
        _service = service;
        _environment = environment;
    }

    // ============================
    // GET PAGINADO + ORDEN
    // ============================
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProfesionalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? busqueda,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "nombre",
        [FromQuery] string? sortDir = "asc")
    {
        var result = await _service.GetPagedAsync(
            busqueda,
            page,
            pageSize,
            sortBy,
            sortDir);

        return Ok(result);
    }

    // ============================
    // GET BY ID
    // ============================
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProfesionalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var profesional = await _service.GetByIdAsync(id);
        return profesional == null ? NotFound() : Ok(profesional);
    }

    // ============================
    // POST
    // ============================
    [HttpPost]
    [ProducesResponseType(typeof(ProfesionalDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(ProfesionalCreateDto dto)
    {
        var profesional = await _service.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = profesional.Id },
            profesional);
    }

    // ============================
    // PUT
    // ============================
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Put(int id, ProfesionalUpdateDto dto)
    {
        await _service.ActualizarAsync(id, dto);
        return NoContent();
    }

    // ============================
    // PUT PERFIL
    // ============================
    [HttpPut("{id}/perfil")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutPerfil(int id, [FromBody] ProfesionalPerfilUpdateDto dto)
    {
        await _service.ActualizarPerfilAsync(id, dto);
        return NoContent();
    }

    // ============================
    // POST FOTO
    // ============================
        [HttpPost("{id}/foto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadFoto(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No se ha enviado ningún archivo.");

                // Validar extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Formato de imagen no válido.");

                // Determine Root Path safely
                string webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // Crear carpeta si no existe
                string uploadFolder = Path.Combine(webRootPath, "uploads", "perfiles");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // Generar nombre unico
                string uniqueFileName = $"{id}_{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var publicUrl = $"{baseUrl}/uploads/perfiles/{uniqueFileName}";

                await _service.ActualizarFotoAsync(id, publicUrl);

                return Ok(new { url = publicUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] UploadFoto: {ex}");
                return StatusCode(500, $"Error interno al subir foto: {ex.Message}");
            }
        }

    // ============================
    // DELETE LÓGICO
    // ============================
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }

    // ============================
    // REACTIVAR
    // ============================
    [HttpPut("reactivar/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reactivar(int id)
    {
        await _service.ReactivarAsync(id);
        return NoContent();
    }
}
