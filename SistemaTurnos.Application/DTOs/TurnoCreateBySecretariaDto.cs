using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaTurnos.Application.DTOs;

public class TurnoCreateBySecretariaDto
{
    [Required]
    public int ProfesionalId { get; set; }

    [Required]
    public int ServicioId { get; set; }

    [Required]
    public DateTime FechaHoraInicio { get; set; }

    [Required]
    public string PacienteDni { get; set; } = null!;

    public string? PacienteNombre { get; set; }

    public string? PacienteObraSocial { get; set; }
    
    public string? MotivoConsulta { get; set; }
}
