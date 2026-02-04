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
    public string? ObraSocial { get; set; }

    public Profesional? Profesional { get; set; }

    // Security: account lockout fields
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public Persona() { }

    public Persona(string nombre, string dni, string email, string passwordHash, Rol rol)
    {
        Nombre = nombre;
        Dni = dni;
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }
}
