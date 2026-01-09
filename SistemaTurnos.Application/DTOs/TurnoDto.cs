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
}
