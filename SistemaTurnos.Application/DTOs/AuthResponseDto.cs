namespace SistemaTurnos.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public PersonaDto User { get; set; } = null!;
    }
}