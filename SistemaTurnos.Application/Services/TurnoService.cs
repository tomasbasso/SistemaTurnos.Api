using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Exceptions;
using System.Linq;

public class TurnoService
{
    private readonly ITurnoRepository _turnos;
    private readonly IPersonaRepository _personas;
    private readonly IProfesionalRepository _profesionales;
    private readonly IServicioRepository _servicios;
    private readonly IHorarioTrabajoRepository _horarios;
    private readonly IBloqueoTiempoRepository _bloqueos;

    public TurnoService(
        ITurnoRepository turnos,
        IPersonaRepository personas,
        IProfesionalRepository profesionales,
        IServicioRepository servicios,
        IHorarioTrabajoRepository horarios,
        IBloqueoTiempoRepository bloqueos)
    {
        _turnos = turnos;
        _personas = personas;
        _profesionales = profesionales;
        _servicios = servicios;
        _horarios = horarios;
        _bloqueos = bloqueos;
    }

    public async Task<Turno> CrearAsync(TurnoCreateDto dto, int personaId)
    {
        if (dto.FechaHoraInicio < DateTime.Now)
            throw new BusinessException(
                "No se pueden crear turnos en el pasado"
            );

        var persona = await _personas.GetByIdAsync(personaId);
        if (persona == null || !persona.Activo)
            throw new BusinessException("Persona inválida");

        var profesional = await _profesionales.GetByIdAsync(dto.ProfesionalId);
        if (profesional == null || !profesional.Activo)
            throw new BusinessException("Profesional inválido");

        var servicio = await _servicios.GetByIdAsync(dto.ServicioId);
        if (servicio == null || !servicio.Activo)
            throw new BusinessException("Servicio inválido");

        // Validar que el profesional ofrezca el servicio
        if (!profesional.Servicios.Any(s => s.Id == dto.ServicioId))
        {
            throw new BusinessException("El profesional seleccionado no ofrece el servicio solicitado.");
        }

        // Validar contra el horario de trabajo del profesional
        var diaSemana = dto.FechaHoraInicio.DayOfWeek;
        var horaInicioTurno = TimeOnly.FromDateTime(dto.FechaHoraInicio);
        var horariosProfesional = await _horarios.GetByProfesionalIdAsync(profesional.Id);

        var horarioDelDia = horariosProfesional.FirstOrDefault(h => h.DiaSemana == diaSemana && h.Activo);

        if (horarioDelDia == null)
        {
            throw new BusinessException($"El profesional no trabaja el día {diaSemana}.");
        }

        var fechaFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);
        var horaFinTurno = TimeOnly.FromDateTime(fechaFin);

        if (horaInicioTurno < horarioDelDia.HoraInicio || horaFinTurno > horarioDelDia.HoraFin)
        {
            throw new BusinessException($"El turno está fuera del horario de trabajo del profesional ({horarioDelDia.HoraInicio:HH:mm} - {horarioDelDia.HoraFin:HH:mm}).");
        }

        // Validar solapamiento con otros turnos
        var solapadoTurno = await _turnos.ExisteSolapamiento(
            dto.ProfesionalId,
            dto.FechaHoraInicio,
            fechaFin
        );

        if (solapadoTurno)
            throw new BusinessException(
                "El profesional ya tiene un turno en ese horario"
            );

        // Validar solapamiento con bloqueos de tiempo
        var solapadoBloqueo = await _bloqueos.ExisteSolapamiento(
            dto.ProfesionalId,
            dto.FechaHoraInicio,
            fechaFin
        );

        if (solapadoBloqueo)
            throw new BusinessException(
                "El profesional tiene un bloqueo de tiempo en ese horario"
            );

        var turno = new Turno(
            personaId,
            dto.ProfesionalId,
            dto.ServicioId,
            dto.FechaHoraInicio,
            servicio.DuracionMinutos
        );

        await _turnos.AddAsync(turno);
        return turno;
    }
    public async Task FinalizarAsync(int turnoId)
    {
        var turno = await _turnos.GetByIdAsync(turnoId)
            ?? throw new NotFoundException("Turno no encontrado");

        turno.Finalizar();

        await _turnos.UpdateAsync(turno);
    }
    public async Task CancelarAsync(int turnoId)
    {
        var turno = await _turnos.GetByIdAsync(turnoId);
        if (turno == null)
            throw new NotFoundException("Turno no encontrado");

        turno.Cancelar();
        await _turnos.UpdateAsync(turno);
    }
    public async Task<IEnumerable<AgendaTurnoDto>> ObtenerAgendaProfesionalAsync(
      int profesionalId,
      DateTime? desde,
      DateTime? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new BusinessException("El rango de fechas es inválido");

        var profesional = await _profesionales.GetByIdAsync(profesionalId);
        if (profesional == null || !profesional.Activo)
            throw new BusinessException("Profesional inválido");

        var turnos = await _turnos.GetAgendaProfesionalAsync(
            profesionalId,
            desde,
            hasta
        );

        return turnos.Select(t => new AgendaTurnoDto
        {
            TurnoId = t.Id,
            FechaHoraInicio = t.FechaHoraInicio,
            FechaHoraFin = t.FechaHoraFin,
            Estado = t.Estado.ToString(),
            PersonaNombre = t.Persona?.Nombre ?? "Desconocido",
            ServicioNombre = t.Servicio?.Nombre ?? "Desconocido"
        });
    }
    public async Task<Turno> GetByIdAsync(int id)
    {
        return await _turnos.GetByIdAsync(id)
            ?? throw new NotFoundException("Turno no encontrado");
    }

    public async Task<IEnumerable<Turno>> GetTurnosByPersonaIdAsync(int personaId)
    {
        return await _turnos.GetByPersonaIdAsync(personaId);
    }

    public async Task<IEnumerable<Turno>> GetAllTurnosAsync()
    {
        return await _turnos.GetAllAsync();
    }
}
