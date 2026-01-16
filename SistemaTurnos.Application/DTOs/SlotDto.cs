namespace SistemaTurnos.Application.DTOs
{
    public class SlotDto
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public bool Disponible { get; set; }
    }
}
