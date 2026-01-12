using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTurnos.Application.Interfaces.Services;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Uncomment if security is needed
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("diario")]
        public async Task<IActionResult> GetReporteDiario([FromQuery] DateTime fecha)
        {
            var reporte = await _reporteService.ObtenerReporteDiarioAsync(fecha);
            return Ok(reporte);
        }

        [HttpGet("mensual")]
        public async Task<IActionResult> GetReporteMensual([FromQuery] int mes, [FromQuery] int anio)
        {
            var reporte = await _reporteService.ObtenerReporteMensualAsync(mes, anio);
            return Ok(reporte);
        }
    }
}
