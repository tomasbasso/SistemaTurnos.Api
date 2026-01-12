using System;
using System.Collections.Generic;

namespace SistemaTurnos.Application.DTOs
{
    public class ReporteDiarioProfesionalDto
    {
        public int ProfesionalId { get; set; }
        public string NombreProfesional { get; set; }
        public DateTime Fecha { get; set; }
        public int TotalTurnos { get; set; }
        public int TurnosPendientes { get; set; }
        public int TurnosConfirmados { get; set; }
        public int TurnosCancelados { get; set; }
        public List<TurnoDto> Turnos { get; set; }
    }
}
