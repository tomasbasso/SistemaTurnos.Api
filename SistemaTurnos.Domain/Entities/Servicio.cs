namespace SistemaTurnos.Domain.Entities
{
    public class Servicio
    {
        public int Id { get; private set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;

        protected Servicio() { } // EF

        public Servicio(string nombre, string descripcion, int duracionMinutos, decimal precio)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            DuracionMinutos = duracionMinutos;
            Precio = precio;
            Activo = true;
        }

        public void Actualizar(string nombre, string descripcion, int duracionMinutos, decimal precio)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            DuracionMinutos = duracionMinutos;
            Precio = precio;
        }
    }
}
