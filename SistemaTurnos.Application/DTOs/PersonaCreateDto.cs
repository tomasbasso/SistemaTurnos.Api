using SistemaTurnos.Domain.Enums;

namespace SistemaTurnos.Application.DTOs
{
    public class PersonaCreateDto
    {
        public string Nombre { get; set; } = null!;

        public string Dni { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public Rol Rol { get; set; }
    }
}
