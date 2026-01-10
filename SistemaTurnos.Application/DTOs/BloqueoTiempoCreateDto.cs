using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaTurnos.Application.DTOs
{
    public class BloqueoTiempoCreateDto
    {
        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        public string? Motivo { get; set; }
    }
}
