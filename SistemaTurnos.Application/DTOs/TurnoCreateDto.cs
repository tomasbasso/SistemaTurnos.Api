namespace SistemaTurnos.Application.DTOs;

public class TurnoCreateDto
{
    public int PersonaId { get; set; }
    public int ProfesionalId { get; set; }
    public int ServicioId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
}
