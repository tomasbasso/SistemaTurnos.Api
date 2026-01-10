using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly SistemaTurnosDbContext _context;

    public ProfesionalRepository(SistemaTurnosDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteMatriculaAsync(string matricula, int? excluirId = null)
    {
        return await _context.Profesionales.AnyAsync(p =>
            p.Matricula == matricula &&
            (!excluirId.HasValue || p.Id != excluirId));
    }

    public async Task<bool> ExisteAsignacion(int personaId)
    {
        return await _context.Profesionales.AnyAsync(p => p.PersonaId == personaId);
    }

    public async Task AddAsync(Profesional profesional)
    {
        _context.Profesionales.Add(profesional);
        await _context.SaveChangesAsync();
    }

    public async Task<Profesional?> GetByIdAsync(int id)
    {
        return await _context.Profesionales
            .Include(p => p.Persona)
            .Include(p => p.Servicios)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Profesional> Items, int Total)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var query = _context.Profesionales
            .Include(p => p.Persona)
            .Where(p => p.Activo)
            .AsQueryable();

        // 🔍 Filtro
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p =>
                p.Persona.Nombre.Contains(busqueda) ||
                p.Matricula.Contains(busqueda));
        }

        // 🔃 Orden dinámico
        query = sortBy?.ToLower() switch
        {
            "nombre" => sortDir == "desc"
                ? query.OrderByDescending(p => p.Persona.Nombre)
                : query.OrderBy(p => p.Persona.Nombre),

            "matricula" => sortDir == "desc"
                ? query.OrderByDescending(p => p.Matricula)
                : query.OrderBy(p => p.Matricula),

            _ => query.OrderBy(p => p.Persona.Nombre)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
