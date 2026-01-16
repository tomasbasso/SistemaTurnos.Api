using System;

namespace SistemaTurnos.Domain.Entities
{
    public class ArchivoAdjunto
    {
        public int Id { get; set; }

        public int NotaClinicaId { get; set; }
        public NotaClinica NotaClinica { get; set; } = null!;

        public string RutaArchivo { get; set; } = string.Empty;
        public string NombreOriginal { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty; // MIME Type
        public long TamanioBytes { get; set; }
        public DateTime FechaSubida { get; set; } = DateTime.Now;
    }
}
