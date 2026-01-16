using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SistemaTurnos.Application.DTOs
{
    public class NotaClinicaCreateDto
    {
        public int TurnoId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public bool VisibleParaPaciente { get; set; }
        public List<IFormFile>? Archivos { get; set; }
    }
}
