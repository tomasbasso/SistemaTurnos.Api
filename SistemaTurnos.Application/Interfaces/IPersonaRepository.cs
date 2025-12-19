using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Interfaces
{
    public interface IPersonaRepository
    {
        Task<bool> ExisteDniAsync(string dni, int? excluirId = null);
        Task AddAsync(Persona persona);
        Task<Persona?> GetByIdAsync(int id);
        Task<List<Persona>> GetAllAsync(string? busqueda);
        Task SaveChangesAsync();
    }
}
