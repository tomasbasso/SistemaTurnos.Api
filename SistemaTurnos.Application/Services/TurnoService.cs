using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Exceptions;
using System.Linq;

using SistemaTurnos.Application.Interfaces.Services;

public class TurnoService
{
    private readonly ITurnoRepository _turnos;
    private readonly IPersonaRepository _personas;
    private readonly IProfesionalRepository _profesionales;
    private readonly IServicioRepository _servicios;
    private readonly IHorarioTrabajoRepository _horarios;
    private readonly IBloqueoTiempoRepository _bloqueos;
    private readonly IEmailService _emailService;

    public TurnoService(
        ITurnoRepository turnos,
        IPersonaRepository personas,
        IProfesionalRepository profesionales,
        IServicioRepository servicios,
        IHorarioTrabajoRepository horarios,
        IBloqueoTiempoRepository bloqueos,
        IEmailService emailService)
    {
        _turnos = turnos;
        _personas = personas;
        _profesionales = profesionales;
        _servicios = servicios;
        _horarios = horarios;
        _bloqueos = bloqueos;
        _emailService = emailService;
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
        
        // Notificacion Email Cliente
        var emailBodyCliente = $@"
            <h2>¡Turno Confirmado!</h2>
            <p>Hola <strong>{persona.Nombre}</strong>,</p>
            <p>Tu turno ha sido reservado exitosamente.</p>
            <ul>
                <li><strong>Fecha y Hora:</strong> {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}</li>
                <li><strong>Profesional:</strong> {profesional.Persona?.Nombre ?? "Profesional"}</li>
                <li><strong>Servicio:</strong> {servicio.Nombre}</li>
                <li><strong>Precio:</strong> ${servicio.Precio}</li>
            </ul>
            <p>¡Te esperamos!</p>";

        await _emailService.SendEmailAsync(
            persona.Email, 
            "Confirmación de Turno - Sistema de Turnos", 
            emailBodyCliente
        );

        // Notificacion Email Profesional
        if (profesional.Persona != null)
        {
             var emailBodyProf = $@"
                <h2>Nuevo Turno Reservado</h2>
                <p>Hola <strong>{profesional.Persona.Nombre}</strong>,</p>
                <p>Se ha agendado un nuevo turno.</p>
                <ul>
                    <li><strong>Paciente:</strong> {persona.Nombre}</li>
                    <li><strong>Fecha y Hora:</strong> {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}</li>
                    <li><strong>Servicio:</strong> {servicio.Nombre}</li>
                </ul>";

             await _emailService.SendEmailAsync(
                profesional.Persona.Email,
                "Nuevo Turno en Agenda",
                emailBodyProf
            );
        }

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

         // Notify Client
         if (turno.Persona != null)
         {
            await _emailService.SendEmailAsync(
                turno.Persona.Email,
                "Turno Cancelado",
                $"Hola {turno.Persona.Nombre}, tu turno del {turno.FechaHoraInicio:dd/MM/yyyy HH:mm} ha sido cancelado."
            );
         }

         // Notify Professional
         var profesional = await _profesionales.GetByIdAsync(turno.ProfesionalId);
         if (profesional?.Persona != null)
         {
             await _emailService.SendEmailAsync(
                profesional.Persona.Email,
                "Turno Cancelado",
                $"Hola {profesional.Persona.Nombre}, el turno con {turno.Persona?.Nombre ?? "Paciente"} del {turno.FechaHoraInicio:dd/MM/yyyy HH:mm} ha sido cancelado."
            );
         }
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
    public async Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(int profesionalId, int servicioId, DateTime fecha)
    {
        var profesional = await _profesionales.GetByIdAsync(profesionalId);
        if (profesional == null || !profesional.Activo)
            throw new BusinessException("Profesional inválido");

        var servicio = await _servicios.GetByIdAsync(servicioId);
        if (servicio == null || !servicio.Activo)
            throw new BusinessException("Servicio inválido");

        var diaSemana = fecha.DayOfWeek;
        var horarios = await _horarios.GetByProfesionalIdAsync(profesionalId);
        var horarioDia = horarios.FirstOrDefault(h => h.DiaSemana == diaSemana && h.Activo);

        if (horarioDia == null)
            return Enumerable.Empty<SlotDto>();

        var fechaBase = fecha.Date;
        var inicioLaboral = fechaBase.Add(horarioDia.HoraInicio.ToTimeSpan());
        var finLaboral = fechaBase.Add(horarioDia.HoraFin.ToTimeSpan());

        var turnos = await _turnos.GetAgendaProfesionalAsync(profesionalId, inicioLaboral, finLaboral);
        var bloqueos = await _bloqueos.GetByProfesionalIdAsync(profesionalId, inicioLaboral, finLaboral);

        var slots = new List<SlotDto>();
        var duracion = servicio.DuracionMinutos;
        var cursor = inicioLaboral;

        while (cursor.AddMinutes(duracion) <= finLaboral)
        {
            var finSlot = cursor.AddMinutes(duracion);
            
            bool ocupado = turnos.Any(t => t.FechaHoraInicio < finSlot && t.FechaHoraFin > cursor && t.Estado != SistemaTurnos.Domain.Enums.EstadoTurno.Cancelado) ||
                           bloqueos.Any(b => b.FechaHoraInicio < finSlot && b.FechaHoraFin > cursor);

            if (!ocupado && cursor > DateTime.Now)
            {
               slots.Add(new SlotDto
               {
                   Inicio = cursor,
                   Fin = finSlot,
                   Disponible = true
               });
            }

            cursor = cursor.AddMinutes(duracion);
        }
        
        return slots;
    }
}
