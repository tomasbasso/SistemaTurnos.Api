using System;

namespace SistemaTurnos.Domain.Entities
{
    public class BloqueoTiempo
    {
        public int Id { get; set; }

        public int ProfesionalId { get; set; }
        public Profesional Profesional { get; set; } = null!;

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string? Motivo { get; set; }
    }
}
