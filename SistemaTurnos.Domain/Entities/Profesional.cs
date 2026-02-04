using System.Collections.Generic;

namespace SistemaTurnos.Domain.Entities
{
    public class Profesional
    {
        public int Id { get; set; }
        public string Matricula { get; set; }
        public bool Activo { get; set; } = true;

        public string? FotoUrl { get; set; }
        public string? Descripcion { get; set; }
        public string? Especialidad { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? InstagramUrl { get; set; }

        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = null!;

        public ICollection<HorarioTrabajo> HorariosTrabajo { get; set; } = new List<HorarioTrabajo>();
        public ICollection<BloqueoTiempo> BloqueosTiempo { get; set; } = new List<BloqueoTiempo>();
        public ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();

        public Profesional() { }

        public Profesional(int personaId, string matricula)
        {
            PersonaId = personaId;
            Matricula = matricula;
        }
    }

}
