using System;
using System.Collections.Generic;

namespace SistemaTurnos.Application.DTOs
{
    public class NotaClinicaDto
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool VisibleParaPaciente { get; set; }
        public List<ArchivoAdjuntoDto> Archivos { get; set; } = new List<ArchivoAdjuntoDto>();
    }

    public class ArchivoAdjuntoDto
    {
        public int Id { get; set; }
        public string NombreOriginal { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
