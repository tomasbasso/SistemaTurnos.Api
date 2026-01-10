using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class PersonasController : ControllerBase
{
    private readonly IPersonaService _personaService;

    public PersonasController(IPersonaService personaService)
    {
        _personaService = personaService;
    }

    // ============================
    // POST - Crear persona
    // ============================
    [HttpPost]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] PersonaCreateDto dto)
    {
        var persona = await _personaService.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = persona.Id },
            persona
        );
    }

    // ============================
    // PUT - Actualizar persona
    // ============================
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Put(int id, [FromBody] PersonaUpdateDto dto)
    {
        await _personaService.ActualizarAsync(id, dto);
        return NoContent();
    }

    // ============================
    // DELETE - Borrado lógico
    // ============================
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _personaService.EliminarAsync(id);
        return NoContent();
    }

    // ============================
    // PUT - Reactivar persona
    // ============================
    [HttpPut("reactivar/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int id)
    {
        await _personaService.ReactivarAsync(id);
        return NoContent();
    }

    // ============================
    // GET - Persona por ID
    // ============================
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PersonaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var persona = await _personaService.GetByIdAsync(id);

        return persona == null
            ? NotFound()
            : Ok(persona);
    }

    // ============================
    // GET - Listado / búsqueda
    // ============================
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PersonaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
      [FromQuery] string? busqueda,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? sortBy = "nombre",
      [FromQuery] string? sortDir = "asc")
    {
        var result = await _personaService.GetPagedAsync(
            busqueda,
            page,
            pageSize,
            sortBy,
            sortDir);

        return Ok(result);
    }
}
