using SistemaTurnos.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Repositories
{
    public interface IHorarioTrabajoRepository
    {
        Task<HorarioTrabajo?> GetByIdAsync(int id);
        Task<IEnumerable<HorarioTrabajo>> GetByProfesionalIdAsync(int profesionalId);
        Task AddAsync(HorarioTrabajo horario);
        Task UpdateAsync(HorarioTrabajo horario);
        Task DeleteAsync(HorarioTrabajo horario);
    }
}
