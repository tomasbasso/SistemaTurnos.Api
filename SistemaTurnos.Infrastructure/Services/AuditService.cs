using System;
using System.Threading.Tasks;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;

namespace SistemaTurnos.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly SistemaTurnosDbContext _context;

        public AuditService(SistemaTurnosDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? usuarioId, string accion, string entidad, string detalle, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = entidad,
                Detalle = detalle,
                IpAddress = ipAddress,
                Fecha = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
