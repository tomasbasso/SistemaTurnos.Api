using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Interfaces.Repositories
{
    public interface IPersonaRepository
    {
        Task<bool> ExisteDniAsync(string dni, int? id = null);
        Task AddAsync(Persona persona);
        Task<Persona?> GetByIdAsync(int id);
        Task<List<Persona>> GetAllAsync(string? busqueda);
        Task SaveChangesAsync();

        Task<(List<Persona> Items, int Total)>
        GetPagedAsync(
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir
    );

    }
}
