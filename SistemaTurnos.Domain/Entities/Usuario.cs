namespace SistemaTurnos.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    public Persona Persona { get; set; } = null!;

    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int Rol { get; set; }
    public bool Activo { get; set; }
    public int PersonaId { get; set; }
}
