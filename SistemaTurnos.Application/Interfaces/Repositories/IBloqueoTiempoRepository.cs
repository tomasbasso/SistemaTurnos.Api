using SistemaTurnos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Repositories
{
    public interface IBloqueoTiempoRepository
    {
        Task<BloqueoTiempo?> GetByIdAsync(int id);
        Task<IEnumerable<BloqueoTiempo>> GetByProfesionalIdAsync(int profesionalId, DateTime desde, DateTime hasta);
        Task AddAsync(BloqueoTiempo bloqueo);
        Task DeleteAsync(BloqueoTiempo bloqueo);
        Task<bool> ExisteSolapamiento(int profesionalId, DateTime inicio, DateTime fin);
    }
}
