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
            NombreServicio = turno.Servicio?.Nombre,
            FotoProfesional = turno.Profesional?.FotoUrl,
            EspecialidadProfesional = turno.Profesional?.Especialidad,
            DescripcionProfesional = turno.Profesional?.Descripcion,
            MatriculaProfesional = turno.Profesional?.Matricula,
            DniPaciente = turno.Persona?.Dni,
            DuracionMinutos = turno.Servicio?.DuracionMinutos ?? 0,
            Precio = turno.Servicio?.Precio ?? 0,
            MotivoConsulta = turno.MotivoConsulta
        };
    }
}
