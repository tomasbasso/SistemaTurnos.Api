using SistemaTurnos.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IProfesionalServicioService
    {
        Task<IEnumerable<ServicioDto>> GetServiciosByProfesionalAsync(int profesionalId);
        Task AsignarServicioAsync(int profesionalId, int servicioId);
        Task RemoverServicioAsync(int profesionalId, int servicioId);
    }
}
