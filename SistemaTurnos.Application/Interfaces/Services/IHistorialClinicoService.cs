using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaTurnos.Application.DTOs;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IHistorialClinicoService
    {
        Task<NotaClinicaDto> CrearNotaAsync(NotaClinicaCreateDto dto);
        Task<IEnumerable<NotaClinicaDto>> GetByTurnoIdAsync(int turnoId);
        Task<IEnumerable<NotaClinicaDto>> GetByPersonaIdAsync(int personaId);
    }
}
