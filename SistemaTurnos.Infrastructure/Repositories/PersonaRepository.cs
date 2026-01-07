using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;

public class PersonaRepository : IPersonaRepository
{
    private readonly SistemaTurnosDbContext _context;

    public PersonaRepository(SistemaTurnosDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Persona>, int)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        IQueryable<Persona> query = _context.Personas
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p =>
                p.Nombre.Contains(busqueda) ||
                p.Dni.Contains(busqueda));
        }

        var total = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "dni" => sortDir == "desc"
                ? query.OrderByDescending(p => p.Dni)
                : query.OrderBy(p => p.Dni),

            _ => sortDir == "desc"
                ? query.OrderByDescending(p => p.Nombre)
                : query.OrderBy(p => p.Nombre)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Persona persona)
    {
        await _context.Personas.AddAsync(persona);
    }

    public async Task<Persona?> GetByIdAsync(int id)
    {
        return await _context.Personas.FindAsync(id);
    }

    public async Task<List<Persona>> GetAllAsync(string? busqueda)
    {
        var query = _context.Personas.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.Nombre.Contains(busqueda));

        return await query.ToListAsync();
    }

    public async Task<bool> ExisteDniAsync(string dni, int? id = null)
    {
        return await _context.Personas.AnyAsync(p =>
            p.Dni == dni &&
            (id == null || p.Id != id));
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
