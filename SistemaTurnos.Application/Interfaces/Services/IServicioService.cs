using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IServicioService
    {
        Task<ServicioDto> CrearAsync(ServicioCreateDto dto);
        Task ActualizarAsync(int id, ServicioUpdateDto dto);
        Task EliminarAsync(int id);
        Task ReactivarAsync(int id);
        Task<ServicioDto?> GetByIdAsync(int id);
        Task<PagedResultDto<ServicioDto>> GetPagedAsync(
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir);
    }
}
