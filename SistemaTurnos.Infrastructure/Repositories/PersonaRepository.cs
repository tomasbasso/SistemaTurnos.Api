using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Infrastructure.Persistence;

namespace SistemaTurnos.Infrastructure.Repositories
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly SistemaTurnosDbContext _context;

        public PersonaRepository(SistemaTurnosDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteDniAsync(string dni, int? excluirId = null)
        {
            return await _context.Personas.AnyAsync(p =>
                p.Dni == dni && (!excluirId.HasValue || p.Id != excluirId));
        }

        public async Task AddAsync(Persona persona)
        {
            _context.Personas.Add(persona);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsDniDuplicado(ex))
            {
                throw new BusinessException($"El DNI {persona.Dni} ya existe");
            }
        }

        public async Task<Persona?> GetByIdAsync(int id)
        {
            return await _context.Personas.FindAsync(id);
        }

        public async Task<List<Persona>> GetAllAsync(string? busqueda)
        {
            var query = _context.Personas
                .Where(p => p.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Dni.Contains(busqueda));
            }

            return await query.ToListAsync();
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();

        // =========================
        // Helpers privados
        // =========================
        private static bool EsDniDuplicado(DbUpdateException ex)
            => ex.InnerException?.Message.Contains("IX_Personas_Dni") == true
            || ex.InnerException?.Message.Contains("UX_Personas_Dni") == true;
    }
}
