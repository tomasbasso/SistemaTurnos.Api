using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;

namespace SistemaTurnos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly SistemaTurnosDbContext _context;
        private readonly IAuditService _auditService;

        public AdminController(SistemaTurnosDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.Today;
            
            var turnosHoy = await _context.Turnos
                .CountAsync(t => t.FechaHoraInicio.Date == today && t.Estado != SistemaTurnos.Domain.Enums.EstadoTurno.Cancelado);

            var profesionalesActivos = await _context.Profesionales
                .CountAsync(p => p.Activo);

            var totalPacientes = await _context.Personas
                .CountAsync(p => p.Rol == SistemaTurnos.Domain.Enums.Rol.Cliente);

            // Calculate estimated revenue for today
            var ingresosHoy = await _context.Turnos
                .Where(t => t.FechaHoraInicio.Date == today && t.Estado != SistemaTurnos.Domain.Enums.EstadoTurno.Cancelado)
                .SumAsync(t => t.Servicio.Precio);

            return Ok(new 
            {
                TurnosHoy = turnosHoy,
                ProfesionalesActivos = profesionalesActivos,
                TotalPacientes = totalPacientes,
                IngresosHoy = ingresosHoy
            });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = items
            });
        }
    }
}
