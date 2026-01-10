using System;

namespace SistemaTurnos.Application.DTOs
{
    public class BloqueoTiempoDto
    {
        public int Id { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string? Motivo { get; set; }
    }
}
