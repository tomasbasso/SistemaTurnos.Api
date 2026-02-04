using System;

namespace SistemaTurnos.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? UsuarioId { get; set; } // Nullable, as system actions might not have a user
        public string Accion { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
        public string Entidad { get; set; } = string.Empty; // Turno, Profesional, etc.
        public string? Detalle { get; set; } // JSON or text description
        public string? IpAddress { get; set; }
    }
}
