using System;
using System.Collections.Generic;

namespace SistemaTurnos.Domain.Entities
{
    public class NotaClinica
    {
        public int Id { get; set; }
        
        public int TurnoId { get; set; }
        public Turno Turno { get; set; } = null!;

        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public bool VisibleParaPaciente { get; set; } = false;

        public ICollection<ArchivoAdjunto> ArchivosAdjuntos { get; set; } = new List<ArchivoAdjunto>();
    }
}
