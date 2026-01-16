using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api/historial")]
    [Authorize] // Require auth
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialClinicoService _service;

        public HistorialController(IHistorialClinicoService service)
        {
            _service = service;
        }

        [HttpPost("turnos/{turnoId}")]
        [Authorize(Roles = "Profesional,Administrador")] // Only Pros/Admin can write notes
        public async Task<IActionResult> CrearNota(int turnoId, [FromForm] NotaClinicaCreateDto dto)
        {
            if (turnoId != dto.TurnoId && dto.TurnoId == 0) dto.TurnoId = turnoId;
            
            var nota = await _service.CrearNotaAsync(dto);
            return Ok(nota);
        }

        [HttpGet("turnos/{turnoId}")]
        public async Task<IActionResult> GetNotasPorTurno(int turnoId)
        {
            var notas = await _service.GetByTurnoIdAsync(turnoId);
            return Ok(notas);
        }

        [HttpGet("pacientes/{personaId}")]
        [Authorize(Roles = "Profesional,Administrador")] // Privacy
        public async Task<IActionResult> GetHistorialPaciente(int personaId)
        {
            var historial = await _service.GetByPersonaIdAsync(personaId);
            return Ok(historial);
        }
    }
}
