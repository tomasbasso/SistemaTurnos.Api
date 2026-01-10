using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Infrastructure.Persistence;
using System;

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
        return await _context.Turnos.FindAsync(id);
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

    public async Task<IEnumerable<Turno>> GetAgendaProfesionalAsync(
      int profesionalId,
      DateTime? desde,
      DateTime? hasta)
    {
        var query = _context.Turnos
            .Where(t =>
                t.ProfesionalId == profesionalId &&
                t.Estado != EstadoTurno.Cancelado
            );

        if (desde.HasValue)
            query = query.Where(t => t.FechaHoraInicio >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(t => t.FechaHoraInicio <= hasta.Value);

        return await query
            .Include(t => t.Persona)
            .Include(t => t.Servicio)
            .OrderBy(t => t.FechaHoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetByFecha(DateTime fecha)
    {
        var desde = fecha.Date;
        var hasta = desde.AddDays(1);

        return await _context.Turnos
            .Where(t =>
                t.FechaHoraInicio >= desde &&
                t.FechaHoraInicio < hasta)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetByPersonaIdAsync(int personaId)
    {
        return await _context.Turnos
            .Where(t => t.PersonaId == personaId)
            .Include(t => t.Profesional)
            .Include(t => t.Servicio)
            .OrderBy(t => t.FechaHoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetAllAsync()
    {
        return await _context.Turnos
            .Include(t => t.Persona)
            .Include(t => t.Profesional)
            .Include(t => t.Servicio)
            .OrderBy(t => t.FechaHoraInicio)
            .ToListAsync();
    }
}
