using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Infrastructure.Repositories
{
    public class HorarioTrabajoRepository : IHorarioTrabajoRepository
    {
        private readonly SistemaTurnosDbContext _context;

        public HorarioTrabajoRepository(SistemaTurnosDbContext context)
        {
            _context = context;
        }

        public async Task<HorarioTrabajo?> GetByIdAsync(int id)
        {
            return await _context.HorariosTrabajo.FindAsync(id);
        }

        public async Task<IEnumerable<HorarioTrabajo>> GetByProfesionalIdAsync(int profesionalId)
        {
            return await _context.HorariosTrabajo
                .Where(h => h.ProfesionalId == profesionalId)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task AddAsync(HorarioTrabajo horario)
        {
            await _context.HorariosTrabajo.AddAsync(horario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HorarioTrabajo horario)
        {
            _context.HorariosTrabajo.Update(horario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(HorarioTrabajo horario)
        {
            _context.HorariosTrabajo.Remove(horario);
            await _context.SaveChangesAsync();
        }
    }
}
