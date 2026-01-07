using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaTurnos.Domain.Entities
{
    public class Profesional
    {
        public int Id { get; private set; }
        public string Nombre { get; set; }
        public string Matricula { get; set; }
        public bool Activo { get; set; } = true;

        protected Profesional() { }

        public Profesional(string nombre, string matricula)
        {
            Nombre = nombre;
            Matricula = matricula;
        }
    }

}
