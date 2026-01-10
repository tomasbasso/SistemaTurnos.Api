using System.ComponentModel.DataAnnotations;

namespace SistemaTurnos.Application.DTOs
{
    public class ProfesionalCreateDto
    {
        [Required]
        public int PersonaId { get; set; }

        [Required]
        public string Matricula { get; set; }
    }
}

