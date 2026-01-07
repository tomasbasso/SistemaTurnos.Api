using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Infrastructure.Persistence;

public class TurnoRepository : ITurnoRepository
{
    private readonly SistemaTurnosDbContext _context;

    public TurnoRepository(SistemaTurnosDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Turno turno)
    {
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Turno turno)
    {
        _context.Turnos.Update(turno);
        await _context.SaveChangesAsync();
    }

    public async Task<Turno?> GetByIdAsync(int id)
    {
        return await _context.Turnos
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> ExisteSolapamiento(
        int profesionalId,
        DateTime inicio,
        DateTime fin)
    {
        return await _context.Turnos.AnyAsync(t =>
            t.ProfesionalId == profesionalId &&
            t.Estado == EstadoTurno.Activo &&
            inicio < t.FechaHoraFin &&
            fin > t.FechaHoraInicio
        );
    }

    public async Task<IEnumerable<Turno>> GetAgendaProfesional(
        int profesionalId,
        DateTime desde,
        DateTime hasta)
    {
        return await _context.Turnos
            .Where(t =>
                t.ProfesionalId == profesionalId &&
                t.FechaHoraInicio >= desde &&
                t.FechaHoraInicio <= hasta)
            .OrderBy(t => t.FechaHoraInicio)
            .ToListAsync();
    }
}
