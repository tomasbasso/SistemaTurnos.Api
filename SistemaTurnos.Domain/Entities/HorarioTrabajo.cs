using SistemaTurnos.Domain.Entities;
using System;

namespace SistemaTurnos.Domain.Entities
{
    public class HorarioTrabajo
    {
        public int Id { get; set; }

        public int ProfesionalId { get; set; }
        public Profesional Profesional { get; set; } = null!;

        public DayOfWeek DiaSemana { get; set; }

        public TimeOnly HoraInicio { get; set; }

        public TimeOnly HoraFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}
