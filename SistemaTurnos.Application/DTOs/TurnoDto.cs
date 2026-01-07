using SistemaTurnos.Domain.Enums;

namespace SistemaTurnos.Application.DTOs;

public class TurnoDto
{
    public int Id { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public EstadoTurno Estado { get; set; }
}
