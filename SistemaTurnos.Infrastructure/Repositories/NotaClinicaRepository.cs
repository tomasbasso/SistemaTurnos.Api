using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Infrastructure.Repositories
{
    public class NotaClinicaRepository : INotaClinicaRepository
    {
        private readonly SistemaTurnosDbContext _context;

        public NotaClinicaRepository(SistemaTurnosDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NotaClinica nota)
        {
            _context.NotasClinicas.Add(nota);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotaClinica>> GetByTurnoIdAsync(int turnoId)
        {
            return await _context.NotasClinicas
                .Include(n => n.ArchivosAdjuntos)
                .Where(n => n.TurnoId == turnoId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotaClinica>> GetByPersonaIdAsync(int personaId)
        {
            return await _context.NotasClinicas
                .Include(n => n.ArchivosAdjuntos)
                .Include(n => n.Turno)
                .Where(n => n.Turno.PersonaId == personaId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
