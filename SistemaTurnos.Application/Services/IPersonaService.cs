using SistemaTurnos.Application.DTOs;

namespace SistemaTurnos.Application.Interfaces
{
    public interface IPersonaService
    {
        // Crear
        Task<PersonaDto> CrearAsync(PersonaCreateDto dto);

        // Actualizar
        Task ActualizarAsync(int id, PersonaCreateDto dto);

        // Borrado lógico
        Task EliminarAsync(int id);

        // Reactivar
        Task ReactivarAsync(int id);

        // Obtener por ID
        Task<PersonaDto?> GetByIdAsync(int id);

        // Listado / búsqueda
        Task<List<PersonaDto>> GetAllAsync(string? busqueda);
    }
}
