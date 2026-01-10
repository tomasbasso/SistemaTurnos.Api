using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Administrador, Profesional")]
    public class HorariosTrabajoController : ControllerBase
    {
        private readonly IHorarioTrabajoService _horarioService;

        public HorariosTrabajoController(IHorarioTrabajoService horarioService)
        {
            _horarioService = horarioService;
        }

        // TODO: Add more granular authorization. A professional should only be able to manage their own schedule.
        // An admin can manage anyone's. This currently allows any professional to manage any schedule.
        [HttpGet("profesionales/{profesionalId}/horarios")]
        public async Task<IActionResult> GetByProfesional(int profesionalId)
        {
            var horarios = await _horarioService.GetByProfesionalIdAsync(profesionalId);
            return Ok(horarios);
        }

        [HttpPost("profesionales/{profesionalId}/horarios")]
        public async Task<IActionResult> Create(int profesionalId, [FromBody] HorarioTrabajoCreateDto createDto)
        {
            var nuevoHorario = await _horarioService.CreateAsync(profesionalId, createDto);
            return CreatedAtAction(nameof(GetByProfesional), new { profesionalId = nuevoHorario.ProfesionalId }, nuevoHorario);
        }

        [HttpPut("horarios/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HorarioTrabajoCreateDto updateDto)
        {
            await _horarioService.UpdateAsync(id, updateDto);
            return NoContent();
        }

        [HttpDelete("horarios/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _horarioService.DeleteAsync(id);
            return NoContent();
        }
    }
}
