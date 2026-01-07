using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;

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

    public async Task AddAsync(Profesional profesional)
    {
        _context.Profesionales.Add(profesional);
        await _context.SaveChangesAsync();
    }

    public async Task<Profesional?> GetByIdAsync(int id)
    {
        return await _context.Profesionales.FindAsync(id);
    }

    public async Task<(List<Profesional> Items, int Total)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var query = _context.Profesionales
            .Where(p => p.Activo)
            .AsQueryable();

        // 🔍 Filtro
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p =>
                p.Nombre.Contains(busqueda) ||
                p.Matricula.Contains(busqueda));
        }

        // 🔃 Orden dinámico
        query = sortBy?.ToLower() switch
        {
            "nombre" => sortDir == "desc"
                ? query.OrderByDescending(p => p.Nombre)
                : query.OrderBy(p => p.Nombre),

            "matricula" => sortDir == "desc"
                ? query.OrderByDescending(p => p.Matricula)
                : query.OrderBy(p => p.Matricula),

            _ => query.OrderBy(p => p.Nombre)
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
