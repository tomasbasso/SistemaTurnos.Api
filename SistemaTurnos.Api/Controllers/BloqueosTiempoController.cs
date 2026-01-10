using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Administrador, Profesional")]
    public class BloqueosTiempoController : ControllerBase
    {
        private readonly IBloqueoTiempoService _bloqueoService;

        public BloqueosTiempoController(IBloqueoTiempoService bloqueoService)
        {
            _bloqueoService = bloqueoService;
        }

        // TODO: Add granular auth check to ensure professional can only see their own blocks.
        [HttpGet("profesionales/{profesionalId}/bloqueos")]
        public async Task<IActionResult> GetByProfesional(int profesionalId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var bloqueos = await _bloqueoService.GetByProfesionalIdAsync(profesionalId, desde, hasta);
            return Ok(bloqueos);
        }

        // TODO: Add granular auth check to ensure professional can only create their own blocks.
        [HttpPost("profesionales/{profesionalId}/bloqueos")]
        public async Task<IActionResult> Create(int profesionalId, [FromBody] BloqueoTiempoCreateDto createDto)
        {
            var nuevoBloqueo = await _bloqueoService.CreateAsync(profesionalId, createDto);
            return Ok(nuevoBloqueo); // Returning Ok for simplicity, could be CreatedAtAction
        }

        // TODO: Add granular auth check to ensure professional can only delete their own blocks.
        [HttpDelete("bloqueos/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _bloqueoService.DeleteAsync(id);
            return NoContent();
        }
    }
}
