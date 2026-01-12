using System;

namespace SistemaTurnos.Application.DTOs;

public class ReporteMensualDto
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int CantidadTurnosTotal { get; set; }
    public int CantidadTurnosRealizados { get; set; }
    public int CantidadTurnosCancelados { get; set; }
    public decimal IngresosEstimados { get; set; }
    public decimal IngresosReales { get; set; }
}
