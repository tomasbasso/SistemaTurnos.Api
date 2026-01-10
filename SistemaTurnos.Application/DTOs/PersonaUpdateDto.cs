using SistemaTurnos.Domain.Enums;

namespace SistemaTurnos.Application.DTOs
{
    public class PersonaUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public Rol? Rol { get; set; }
    }
}
