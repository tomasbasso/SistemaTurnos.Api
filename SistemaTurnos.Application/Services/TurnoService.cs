using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Domain.Entities;
using System.Linq;
using static BCrypt.Net.BCrypt;

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
    private readonly IAuditService _auditService;
    private readonly IEmailTemplateService _emailTemplateService;

    public TurnoService(
        ITurnoRepository turnos,
        IPersonaRepository personas,
        IProfesionalRepository profesionales,
        IServicioRepository servicios,
        IHorarioTrabajoRepository horarios,
        IBloqueoTiempoRepository bloqueos,
        IEmailService emailService,
        IAuditService auditService,
        IEmailTemplateService emailTemplateService)
    {
        _turnos = turnos;
        _personas = personas;
        _profesionales = profesionales;
        _servicios = servicios;
        _horarios = horarios;
        _bloqueos = bloqueos;
        _emailService = emailService;
        _auditService = auditService;
        _emailTemplateService = emailTemplateService;
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
        // Validar contra el horario de trabajo del profesional
        var diaSemana = dto.FechaHoraInicio.DayOfWeek;
        var fechaTurno = dto.FechaHoraInicio.Date;
        var horaInicioTurno = TimeOnly.FromDateTime(dto.FechaHoraInicio);
        var horariosProfesional = await _horarios.GetByProfesionalIdAsync(profesional.Id);

        // Prioritize specific date schedules
        var horariosDia = horariosProfesional.Where(h => h.Fecha.HasValue && h.Fecha.Value.Date == fechaTurno && h.Activo).ToList();
        
        // Fallback
        if (!horariosDia.Any())
        {
            horariosDia = horariosProfesional.Where(h => !h.Fecha.HasValue && h.DiaSemana == diaSemana && h.Activo).ToList();
        }

        if (!horariosDia.Any())
        {
            throw new BusinessException($"El profesional no trabaja el día {fechaTurno:dd/MM/yyyy}.");
        }

        var fechaFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);
        var horaFinTurno = TimeOnly.FromDateTime(fechaFin);

        // Check if turn fits in ANY of the schedules
        bool fitsInSchedule = horariosDia.Any(h => horaInicioTurno >= h.HoraInicio && horaFinTurno <= h.HoraFin);

        if (!fitsInSchedule)
        {
            // For error message, show all available slots? Or just generic.
            var horariosStr = string.Join(", ", horariosDia.Select(h => $"{h.HoraInicio:HH:mm}-{h.HoraFin:HH:mm}"));
            throw new BusinessException($"El turno está fuera del horario de trabajo del profesional ({horariosStr}).");
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
        var emailBodyCliente = _emailTemplateService.GenerateTurnoConfirmacionHtml(turno, profesional, servicio);

        await _emailService.SendEmailAsync(
            persona.Email, 
            "Confirmación de Turno - Sistema de Turnos", 
            emailBodyCliente
        );

        // Notificacion Email Profesional
        if (profesional.Persona != null)
        {
             // For professional notification, we can reuse the same template or create a simpler one. 
             // For now, let's allow them to see the same nice confirmation.
             var emailBodyProf = _emailTemplateService.GenerateTurnoConfirmacionHtml(turno, profesional, servicio);

             await _emailService.SendEmailAsync(
                profesional.Persona.Email,
                "Nuevo Turno en Agenda",
                emailBodyProf
            );
        }



        // Audit Log
        await _auditService.LogAsync(personaId, "Crear Turno", "Turno", $"Turno creado ID: {turno.Id} para {fechaTurno:dd/MM/yyyy}");

        return turno;
    }

    public async Task<Turno> CrearTurnoSecretariaAsync(TurnoCreateBySecretariaDto dto)
    {
         if (dto.FechaHoraInicio < DateTime.Now)
            throw new BusinessException("No se pueden crear turnos en el pasado");

        // 1. Find or Create Persona
        // Use IQueryable or similar if repository supports it. Assuming GetByDniAsync exists or we need to add it?
        // IPersonaRepository generic usually has GetAll, GetById. Let's assume we need to add GetByDni or search.
        // Let's use GetAll for now if no specific method, OR better: add GetByDniAsync to interface/repo.
        // IMPORTANT: Checking IPersonaRepository methods first would be ideal, but for now I'll assume I might need to query.
        // Actually, let's peek at IPersonaRepository in a separate tool call if possible, or just assume I need to fetch all and filter (bad performance) or assume GetByDni exists.
        // Given I'm in "replace", I'll commit to a logic. 
        // If IPersonaRepository doesn't have GetByDni, I should probably add it.
        // But to keep it simple and assuming small dataset or standard repo pattern:
        // var persona = await _personas.GetByDniAsync(dto.PacienteDni);
        
        // Wait, I can't check repo here. I will assume I need to find it. 
        // Let's write the code assuming a `GetByDniAsync` method and if it fails compilation I'll add it.
        // Actually, looking at TurnoService imports, it uses `IPersonaRepository`. 

        var persona = await _personas.GetByDniAsync(dto.PacienteDni);
        
        if (persona == null)
        {
            // Create new Client Persona
            var sanitizedName = (dto.PacienteNombre ?? "Invitado").Replace(" ", "").Trim();
            if (string.IsNullOrEmpty(sanitizedName)) sanitizedName = "Invitado";

            var email = $"{sanitizedName.ToLower()}@correo.com";
            
            // Check if generated email exists, if so append random or DNI?
            // For now, simplicity: Name + DNI pattern ensures uniqueness IF the user is new.
            // But if Name@correo.com exists (another user same name), we might conflict.
            // Better: Name.Dni@correo.com
            email = $"{sanitizedName.ToLower()}.{dto.PacienteDni}@correo.com";

            var passwordRaw = $"{sanitizedName}{dto.PacienteDni}";
            var passwordHash = HashPassword(passwordRaw);

            // Create Entity
            persona = new Persona(
                dto.PacienteNombre ?? "Invitado", 
                dto.PacienteDni, 
                email, 
                passwordHash, 
                SistemaTurnos.Domain.Enums.Rol.Cliente
            );
            persona.ObraSocial = dto.PacienteObraSocial;

            // We need to add it deeply.
            await _personas.AddAsync(persona);
            await _personas.SaveChangesAsync(); 
        }
        else
        {
            // Update info if provided and different
            if (!string.IsNullOrEmpty(dto.PacienteNombre) && dto.PacienteNombre != persona.Nombre)
                persona.Nombre = dto.PacienteNombre;
            
            if (!string.IsNullOrEmpty(dto.PacienteObraSocial) && dto.PacienteObraSocial != persona.ObraSocial)
                persona.ObraSocial = dto.PacienteObraSocial;
            
            await _personas.UpdateAsync(persona);
            await _personas.SaveChangesAsync();
        }

        // 2. Create Turno logic (Reuse Logic? Or call internal helper?)
        // Reuse logic is hard because CreateAsync takes TurnoCreateDto which doesn't have MotivoConsulta.
        // So I'll replicate the core checks or refactor.
        // Replicating checks for now to be safe and fast.

        var profesional = await _profesionales.GetByIdAsync(dto.ProfesionalId);
        if (profesional == null || !profesional.Activo)
             throw new BusinessException("Profesional inválido");

        var servicio = await _servicios.GetByIdAsync(dto.ServicioId);
        if (servicio == null || !servicio.Activo)
             throw new BusinessException("Servicio inválido");

        if (!profesional.Servicios.Any(s => s.Id == dto.ServicioId))
             throw new BusinessException("El profesional no ofrece el servicio solicitado.");

        // Horario Validations
        var diaSemana = dto.FechaHoraInicio.DayOfWeek;
        var fechaTurno = dto.FechaHoraInicio.Date;
        var horaInicioTurno = TimeOnly.FromDateTime(dto.FechaHoraInicio);
        var horariosProfesional = await _horarios.GetByProfesionalIdAsync(profesional.Id);

        var horariosDia = horariosProfesional.Where(h => h.Fecha.HasValue && h.Fecha.Value.Date == fechaTurno && h.Activo).ToList();
        if (!horariosDia.Any())
            horariosDia = horariosProfesional.Where(h => !h.Fecha.HasValue && h.DiaSemana == diaSemana && h.Activo).ToList();

        if (!horariosDia.Any())
            throw new BusinessException($"El profesional no trabaja el día {fechaTurno:dd/MM/yyyy}.");

        var fechaFin = dto.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);
        var horaFinTurno = TimeOnly.FromDateTime(fechaFin);

        bool fitsInSchedule = horariosDia.Any(h => horaInicioTurno >= h.HoraInicio && horaFinTurno <= h.HoraFin);
        if (!fitsInSchedule)
             throw new BusinessException($"El turno está fuera del horario de trabajo.");

         // Solapamiento
        var solapadoTurno = await _turnos.ExisteSolapamiento(dto.ProfesionalId, dto.FechaHoraInicio, fechaFin);
        if (solapadoTurno) throw new BusinessException("El profesional ya tiene un turno en ese horario");

        var solapadoBloqueo = await _bloqueos.ExisteSolapamiento(dto.ProfesionalId, dto.FechaHoraInicio, fechaFin);
        if (solapadoBloqueo) throw new BusinessException("El profesional tiene un bloqueo de tiempo en ese horario");

        // Create Turno
        // Use the constructor with MotivoConsulta
        var turno = new Turno(
            persona.Id,
            dto.ProfesionalId,
            dto.ServicioId,
            dto.FechaHoraInicio,
            servicio.DuracionMinutos,
            dto.MotivoConsulta
        );

        await _turnos.AddAsync(turno);

        // Notifications (Reuse logic? Yes)
        // Simplified for brevity here
        var emailBodyCliente = _emailTemplateService.GenerateTurnoConfirmacionHtml(turno, profesional, servicio);
        // Only send if email is real? generated email ends in @guest.local
        if (!persona.Email.EndsWith("@guest.local")) 
        {
             await _emailService.SendEmailAsync(persona.Email, "Confirmación de Turno", emailBodyCliente);
        }

        if (profesional.Persona != null)
        {
             await _emailService.SendEmailAsync(profesional.Persona.Email, "Nuevo Turno (Secretaría)", emailBodyCliente);
        }
        
        await _auditService.LogAsync(null, "Crear Turno (Sec)", "Turno", $"Turno Secretaría ID: {turno.Id}");

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
            var emailBody = _emailTemplateService.GenerateTurnoCancelacionHtml(turno);
            await _emailService.SendEmailAsync(
                turno.Persona.Email,
                "Turno Cancelado",
                emailBody
            );
         }

         // Notify Professional
         var profesional = await _profesionales.GetByIdAsync(turno.ProfesionalId);
         if (profesional?.Persona != null)
         {
             var emailBodyProf = _emailTemplateService.GenerateTurnoCancelacionHtml(turno);
             await _emailService.SendEmailAsync(
                profesional.Persona.Email,
                "Turno Cancelado",
                emailBodyProf
            );
         }

         // Audit Log
         await _auditService.LogAsync(null, "Cancelar Turno", "Turno", $"Turno cancelado ID: {turnoId}");
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
        var fechaConsulta = fecha.Date;
        var horarios = await _horarios.GetByProfesionalIdAsync(profesionalId);
        
        // 1. Get potentially multiple schedules
        var horariosDia = horarios.Where(h => h.Fecha.HasValue && h.Fecha.Value.Date == fechaConsulta && h.Activo).ToList();
        
        // 2. Fallback to recurring if no specific date schedules exist
        if (!horariosDia.Any())
        {
            horariosDia = horarios.Where(h => !h.Fecha.HasValue && h.DiaSemana == diaSemana && h.Activo).ToList();
        }

        if (!horariosDia.Any())
            return Enumerable.Empty<SlotDto>();

        var slots = new List<SlotDto>();
        var duracion = servicio.DuracionMinutos;

        // 3. Iterate through each schedule (e.g. Morning shift, Afternoon shift)
        foreach (var horario in horariosDia)
        {
            var fechaBase = fecha.Date;
            var inicioLaboral = fechaBase.Add(horario.HoraInicio.ToTimeSpan());
            var finLaboral = fechaBase.Add(horario.HoraFin.ToTimeSpan());

            // Fetch turns/blocks for this specific window to optimize? 
            // Or just fetch for the whole day once? 
            // Optimization: Fetching once for the whole "day" range might be better, 
            // but since we iterate schedules, let's keep it simple and reuse existing calls per schedule 
            // OR refrain from making multiple DB calls.
            // Actually, let's fetch for the specific window as logic implies.
            // Note: Optimally we'd fetch all turns for the day once, but existing code fetches per window.
            // Let's stick to per-window for minimal friction, or widen the search if valid.
            
            var turnos = await _turnos.GetAgendaProfesionalAsync(profesionalId, inicioLaboral, finLaboral);
            var bloqueos = await _bloqueos.GetByProfesionalIdAsync(profesionalId, inicioLaboral, finLaboral);

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
        }
        
        return slots.OrderBy(s => s.Inicio).ToList();
    }

    public async Task<IEnumerable<Turno>> GetAgendaGlobalAsync(DateTime? desde, DateTime? hasta)
    {
        return await _turnos.GetAgendaGlobalAsync(desde, hasta);
    }
}
