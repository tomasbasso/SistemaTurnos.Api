using SistemaTurnos.Domain.Enums;

namespace SistemaTurnos.Domain.Entities;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Rol Rol { get; set; }
    public bool Activo { get; set; }

    public Profesional? Profesional { get; set; }

    public Persona() { }

    public Persona(string nombre, string dni, string email, string passwordHash, Rol rol)
    {
        Nombre = nombre;
        Dni = dni;
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
    }
}
