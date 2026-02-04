using SistemaTurnos.Domain.Enums;

public class TurnoDto
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public int ProfesionalId { get; set; }
    public int ServicioId { get; set; }

    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }

    public EstadoTurno Estado { get; set; }

    public string? NombreProfesional { get; set; }
    public string? NombrePaciente { get; set; }
    public string? NombreServicio { get; set; }
    public string? FotoProfesional { get; set; }
    public string? EspecialidadProfesional { get; set; }
    public string? DescripcionProfesional { get; set; }
    public string? MatriculaProfesional { get; set; }
    public string? DniPaciente { get; set; }
    public int DuracionMinutos { get; set; }
    public decimal Precio { get; set; }
    public string? MotivoConsulta { get; set; }
}
