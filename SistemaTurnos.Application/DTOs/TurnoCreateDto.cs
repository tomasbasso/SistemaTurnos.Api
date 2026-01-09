using System.ComponentModel.DataAnnotations;

public class TurnoCreateDto
{
    [Required]
    public int PersonaId { get; set; }

    [Required]
    public int ProfesionalId { get; set; }

    [Required]
    public int ServicioId { get; set; }

    [Required]
    public DateTime FechaHoraInicio { get; set; }
}