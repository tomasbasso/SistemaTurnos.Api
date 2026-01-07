using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalService _service;

    public ProfesionalesController(IProfesionalService service)
    {
        _service = service;
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
