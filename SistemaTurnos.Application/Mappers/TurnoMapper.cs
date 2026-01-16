using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Entities;

public static class TurnoMapper
{
    public static TurnoDto ToDto(this Turno turno)
    {
        return new TurnoDto
        {
            Id = turno.Id,
            PersonaId = turno.PersonaId,
            ProfesionalId = turno.ProfesionalId,
            ServicioId = turno.ServicioId,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaHoraFin = turno.FechaHoraFin,
            Estado = turno.Estado,
            NombreProfesional = turno.Profesional?.Persona?.Nombre,
            NombrePaciente = turno.Persona?.Nombre,
            NombreServicio = turno.Servicio?.Nombre
        };
    }
}
