namespace SistemaTurnos.Domain.Entities
{
    public class GlobalConfig
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty; // Key
        public string Valor { get; set; } = string.Empty; // Value
        public string? Descripcion { get; set; }
    }
}
