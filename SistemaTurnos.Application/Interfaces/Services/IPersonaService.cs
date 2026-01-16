using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IPersonaService
    {
        Task<PagedResultDto<PersonaDto>> GetPagedAsync(
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir);

        Task<PersonaDto> CrearAsync(PersonaCreateDto dto);
        Task ActualizarAsync(int id, PersonaUpdateDto dto);
        Task EliminarAsync(int id);
        Task ReactivarAsync(int id);
        Task<PersonaDto?> GetByIdAsync(int id);
        Task<List<PersonaDto>> GetAllAsync(string? busqueda);
        Task<PersonaDto?> GetByEmailAsync(string email);
        Task<Persona?> GetPersonaByEmailAsync(string email);
        Task<IEnumerable<PersonaDto>> GetPacientesByProfesionalAsync(int profesionalId);
    }
}
