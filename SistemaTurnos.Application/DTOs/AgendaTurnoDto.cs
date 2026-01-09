namespace SistemaTurnos.Application.DTOs
{
    public class AgendaTurnoDto
    {
        public int TurnoId { get; set; }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string Estado { get; set; } = null!;

        public string PersonaNombre { get; set; } = null!;
        public string ServicioNombre { get; set; } = null!;
    }
}