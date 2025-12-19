using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Domain.Exceptions;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> Post([FromBody] PersonaCreateDto dto)
    {
        try
        {
            var persona = await _personaService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = persona.Id },
                persona
            );
        }
        catch (BusinessException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // ============================
    // PUT - Actualizar persona
    // ============================
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] PersonaCreateDto dto)
    {
        try
        {
            await _personaService.ActualizarAsync(id, dto);
            return NoContent();
        }
        catch (BusinessException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // ============================
    // DELETE - Borrado lógico
    // ============================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _personaService.EliminarAsync(id);
            return NoContent();
        }
        catch (BusinessException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ============================
    // PUT - Reactivar persona
    // ============================
    [HttpPut("reactivar/{id}")]
    public async Task<IActionResult> Reactivar(int id)
    {
        try
        {
            await _personaService.ReactivarAsync(id);
            return NoContent();
        }
        catch (BusinessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ============================
    // GET - Persona por ID
    // ============================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var persona = await _personaService.GetByIdAsync(id);

        if (persona == null)
            return NotFound($"No se encontró la persona con ID {id}");

        return Ok(persona);
    }

    // ============================
    // GET - Listado / búsqueda
    // ============================
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busqueda)
    {
        var personas = await _personaService.GetAllAsync(busqueda);
        return Ok(personas);
    }
}
