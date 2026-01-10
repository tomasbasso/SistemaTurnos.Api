using SistemaTurnos.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IHorarioTrabajoService
    {
        Task<IEnumerable<HorarioTrabajoDto>> GetByProfesionalIdAsync(int profesionalId);
        Task<HorarioTrabajoDto> CreateAsync(int profesionalId, HorarioTrabajoCreateDto createDto);
        Task UpdateAsync(int id, HorarioTrabajoCreateDto updateDto);
        Task DeleteAsync(int id);
    }
}