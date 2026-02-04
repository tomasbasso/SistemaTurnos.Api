using System;

namespace SistemaTurnos.Application.DTOs
{
    public class HorarioTrabajoDto
    {
        public int Id { get; set; }
        public int ProfesionalId { get; set; }
        public DayOfWeek DiaSemana { get; set; }
        public DateTime? Fecha { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
