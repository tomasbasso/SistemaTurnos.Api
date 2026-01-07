using SistemaTurnos.Domain.Entities;

public interface IProfesionalRepository
{
    Task<bool> ExisteMatriculaAsync(string matricula, int? excluirId = null);

    Task AddAsync(Profesional profesional);
    Task<Profesional?> GetByIdAsync(int id);

    Task<(List<Profesional> Items, int Total)> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir);

    Task SaveChangesAsync();
}
