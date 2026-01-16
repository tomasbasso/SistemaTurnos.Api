using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Interfaces.Repositories
{
    public interface INotaClinicaRepository
    {
        Task AddAsync(NotaClinica nota);
        Task<IEnumerable<NotaClinica>> GetByTurnoIdAsync(int turnoId);
        Task<IEnumerable<NotaClinica>> GetByPersonaIdAsync(int personaId);
        Task SaveChangesAsync();
    }
}
