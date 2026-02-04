using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api/profesionales/{profesionalId}/servicios")]
    [Authorize(Roles = "Administrador, Profesional, Cliente, Secretario")] 
    public class ProfesionalServicioController : ControllerBase
    {
        private readonly IProfesionalServicioService _profesionalServicioService;

        public ProfesionalServicioController(IProfesionalServicioService profesionalServicioService)
        {
            _profesionalServicioService = profesionalServicioService;
        }

        // GET /api/profesionales/{profesionalId}/servicios
        [HttpGet]
        public async Task<IActionResult> GetServiciosByProfesional(int profesionalId)
        {
            // TODO: Add granular auth to check if the logged-in professional matches profesionalId
            var servicios = await _profesionalServicioService.GetServiciosByProfesionalAsync(profesionalId);
            return Ok(servicios);
        }

        // POST /api/profesionales/{profesionalId}/servicios/{servicioId}
        [HttpPost("{servicioId}")]
        public async Task<IActionResult> AsignarServicio(int profesionalId, int servicioId)
        {
            // TODO: Add granular auth
            await _profesionalServicioService.AsignarServicioAsync(profesionalId, servicioId);
            return NoContent();
        }

        // DELETE /api/profesionales/{profesionalId}/servicios/{servicioId}
        [HttpDelete("{servicioId}")]
        public async Task<IActionResult> RemoverServicio(int profesionalId, int servicioId)
        {
            // TODO: Add granular auth
            await _profesionalServicioService.RemoverServicioAsync(profesionalId, servicioId);
            return NoContent();
        }
    }
}
