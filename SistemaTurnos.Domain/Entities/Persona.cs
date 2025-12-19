namespace SistemaTurnos.Domain.Entities;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool Activo { get; set; }

    public Persona() { }

    public Persona(string nombre, string dni, string email)
    {
        Nombre = nombre;
        Dni = dni;
        Email = email;
        Activo = true;
    }
}
