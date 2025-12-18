namespace SistemaTurnos.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    public Persona Persona { get; set; } = null!;
}
