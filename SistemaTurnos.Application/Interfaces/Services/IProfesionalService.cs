using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;

public interface IProfesionalService
{
    Task<ProfesionalDto> CrearAsync(ProfesionalCreateDto dto);
    Task ActualizarAsync(int id, ProfesionalUpdateDto dto);
    Task EliminarAsync(int id);
    Task ReactivarAsync(int id);
    Task ActualizarPerfilAsync(int id, ProfesionalPerfilUpdateDto dto);
    Task ActualizarFotoAsync(int id, string fotoUrl);

    Task<ProfesionalDto?> GetByIdAsync(int id);

    Task<PagedResultDto<ProfesionalDto>> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir);
}
