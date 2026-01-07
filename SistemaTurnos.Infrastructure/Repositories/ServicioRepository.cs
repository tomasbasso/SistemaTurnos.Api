using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure.Persistence;

public class ServicioRepository : IServicioRepository
{
    private readonly SistemaTurnosDbContext _context;

    public ServicioRepository(SistemaTurnosDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Servicio servicio)
    {
        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();
    }

    public Task<Servicio?> GetByIdAsync(int id)
    {
        return _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);
    }

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();

    public async Task<(List<Servicio>, int)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var query = _context.Servicios
            .Where(s => s.Activo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(s => s.Nombre.Contains(busqueda));

        query = sortBy?.ToLower() switch
        {
            "duracion" => sortDir == "desc"
                ? query.OrderByDescending(s => s.DuracionMinutos)
                : query.OrderBy(s => s.DuracionMinutos),

            _ => sortDir == "desc"
                ? query.OrderByDescending(s => s.Nombre)
                : query.OrderBy(s => s.Nombre)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
