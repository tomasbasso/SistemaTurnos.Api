namespace SistemaTurnos.Application.DTOs;

public class PersonaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public string Email { get; set; } = null!;
}
