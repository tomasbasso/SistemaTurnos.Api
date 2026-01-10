using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador, Profesional, Cliente")]
public class ServiciosController : ControllerBase
{
    private readonly IServicioService _service;

    public ServiciosController(IServicioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        string? busqueda,
        int page = 1,
        int pageSize = 10,
        string? sortBy = "nombre",
        string? sortDir = "asc")
    {
        var result = await _service.GetPagedAsync(
            busqueda, page, pageSize, sortBy, sortDir);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador, Profesional")]
    public async Task<IActionResult> Post(ServicioCreateDto dto)
    {
        var servicio = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = servicio.Id }, servicio);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador, Profesional")]
    public async Task<IActionResult> Put(int id, ServicioUpdateDto dto)
    {
        await _service.ActualizarAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador, Profesional")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
