using System.ComponentModel.DataAnnotations;

namespace SistemaTurnos.Api.DTOs
{
    public class PersonaCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Dni { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "El formato del email es incorrecto")] // <--- Esto hace la magia automática
        public string Email { get; set; } = null!;
    }
}
