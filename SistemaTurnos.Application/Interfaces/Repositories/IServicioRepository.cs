using SistemaTurnos.Domain.Entities;

public interface IServicioRepository
{
    Task AddAsync(Servicio servicio);
    Task<Servicio?> GetByIdAsync(int id);
    Task SaveChangesAsync();

    Task<(List<Servicio>, int)> GetPagedAsync(
      string? busqueda,
      int page,
      int pageSize,
      string? sortBy,
      string? sortDir);
}
