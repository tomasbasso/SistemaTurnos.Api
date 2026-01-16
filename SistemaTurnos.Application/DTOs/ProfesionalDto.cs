using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.DTOs
{
    public class ProfesionalDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Matricula { get; set; }
        public string? FotoUrl { get; set; }
        public string? Descripcion { get; set; }
        public ICollection<ServicioDto> Servicios { get; set; } = new List<ServicioDto>();
    }

}
