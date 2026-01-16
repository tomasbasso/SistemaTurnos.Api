using SistemaTurnos.Domain.Entities;

public interface IProfesionalRepository
{
    Task<bool> ExisteMatriculaAsync(string matricula, int? excluirId = null);
    Task<bool> ExisteAsignacion(int personaId);

    Task AddAsync(Profesional profesional);
    Task<Profesional?> GetByIdAsync(int id);
    Task<Profesional?> GetByPersonaIdAsync(int personaId);

    Task<(List<Profesional> Items, int Total)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir);

    Task SaveChangesAsync();
}
