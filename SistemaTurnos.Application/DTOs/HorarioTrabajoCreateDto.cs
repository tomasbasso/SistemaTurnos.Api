using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaTurnos.Application.DTOs
{
    public class HorarioTrabajoCreateDto
    {
        [Required]
        public DayOfWeek DiaSemana { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "El formato de HoraInicio debe ser HH:mm")]
        public string HoraInicio { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "El formato de HoraFin debe ser HH:mm")]
        public string HoraFin { get; set; } = string.Empty;
    }
}
