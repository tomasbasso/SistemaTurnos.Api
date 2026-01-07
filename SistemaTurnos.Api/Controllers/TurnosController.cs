using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
[ApiController]
[Route("api/turnos")]
public class TurnosController : ControllerBase
{
    private readonly TurnoService _service;

    public TurnosController(TurnoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(TurnoCreateDto dto)
    {
        var turno = await _service.CrearAsync(dto);
        return Ok(turno);
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _service.CancelarAsync(id);
        return NoContent();
    }

    [HttpGet("profesional/{id}")]
    public async Task<IActionResult> Agenda(
        int id,
        DateTime desde,
        DateTime hasta)
    {
        var agenda = await _service.GetAgendaProfesional(id,
                                                         desde,
                                                         hasta);
        return Ok(agenda);
    }
    [HttpPut("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(int id)
    {
        await _service.FinalizarAsync(id);
        return NoContent();
    }
}
