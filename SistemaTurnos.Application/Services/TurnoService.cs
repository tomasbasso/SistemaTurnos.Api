using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Exceptions;

public class TurnoService
{
    private readonly ITurnoRepository _turnos;
    private readonly IPersonaRepository _personas;
    private readonly IProfesionalRepository _profesionales;
    private readonly IServicioRepository _servicios;

    public TurnoService(
        ITurnoRepository turnos,
        IPersonaRepository personas,
        IProfesionalRepository profesionales,
        IServicioRepository servicios)
    {
        _turnos = turnos;
        _personas = personas;
        _profesionales = profesionales;
        _servicios = servicios;
    }

    public async Task<Turno> CrearAsync(TurnoCreateDto dto)
    {
        if (dto.FechaHoraInicio < DateTime.Now)
            throw new BusinessException(
                "No se pueden crear turnos en el pasado"
            );

        var persona = await _personas.GetByIdAsync(dto.PersonaId);
        if (persona == null || !persona.Activo)
            throw new BusinessException("Persona inválida");

        var profesional = await _profesionales.GetByIdAsync(dto.ProfesionalId);
        if (profesional == null || !profesional.Activo)
            throw new BusinessException("Profesional inválido");

        var servicio = await _servicios.GetByIdAsync(dto.ServicioId);
        if (servicio == null || !servicio.Activo)
            throw new BusinessException("Servicio inválido");

        var fechaFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

        var solapado = await _turnos.ExisteSolapamiento(
            dto.ProfesionalId,
            dto.FechaHoraInicio,
            fechaFin
        );

        if (solapado)
            throw new BusinessException(
                "El profesional ya tiene un turno en ese horario"
            );

        var turno = new Turno(
            dto.PersonaId,
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
}
