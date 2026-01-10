using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Infrastructure.Repositories
{
    public class BloqueoTiempoRepository : IBloqueoTiempoRepository
    {
        private readonly SistemaTurnosDbContext _context;

        public BloqueoTiempoRepository(SistemaTurnosDbContext context)
        {
            _context = context;
        }

        public async Task<BloqueoTiempo?> GetByIdAsync(int id)
        {
            return await _context.BloqueosTiempo.FindAsync(id);
        }

        public async Task<IEnumerable<BloqueoTiempo>> GetByProfesionalIdAsync(int profesionalId, DateTime desde, DateTime hasta)
        {
            return await _context.BloqueosTiempo
                .Where(b => b.ProfesionalId == profesionalId && b.FechaHoraInicio < hasta && b.FechaHoraFin > desde)
                .OrderBy(b => b.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task AddAsync(BloqueoTiempo bloqueo)
        {
            await _context.BloqueosTiempo.AddAsync(bloqueo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(BloqueoTiempo bloqueo)
        {
            _context.BloqueosTiempo.Remove(bloqueo);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteSolapamiento(int profesionalId, DateTime inicio, DateTime fin)
        {
            return await _context.BloqueosTiempo.AnyAsync(b =>
                b.ProfesionalId == profesionalId &&
                inicio < b.FechaHoraFin &&
                fin > b.FechaHoraInicio
            );
        }
    }
}
