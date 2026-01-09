using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Application.Exceptions;
//using SistemaTurnos.Application.Mappers;

[ApiController]
[Route("api/turnos")]
public class TurnosController : ControllerBase
{
    private readonly TurnoService _service;

    public TurnosController(TurnoService service)
    {
        _service = service;
    }

    // --------------------
    // CREAR TURNO
    // --------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] TurnoCreateDto dto)
    {
        var turno = await _service.CrearAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = turno.Id },
            turno.ToDto()
        );
    }

    // --------------------
    // OBTENER POR ID
    // --------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var turno = await _service.GetByIdAsync(id);
        return Ok(turno.ToDto());
    }

    // --------------------
    // CANCELAR
    // --------------------
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _service.CancelarAsync(id);
        return NoContent();
    }

    // --------------------
    // FINALIZAR
    // --------------------
    [HttpPut("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(int id)
    {
        await _service.FinalizarAsync(id);
        return NoContent();
    }

    // --------------------
    // AGENDA PROFESIONAL
    // --------------------
    [HttpGet("profesional/{profesionalId}")]
    public async Task<IActionResult> Agenda(
        int profesionalId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var turnos = await _service.GetAgendaProfesional(
            profesionalId,
            desde,
            hasta
        );

        return Ok(turnos);
    }
}
