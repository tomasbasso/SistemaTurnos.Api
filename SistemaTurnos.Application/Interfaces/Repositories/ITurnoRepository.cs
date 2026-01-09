using SistemaTurnos.Domain.Entities;

public interface ITurnoRepository
{
    Task AddAsync(Turno turno);
    Task UpdateAsync(Turno turno);
    Task<Turno?> GetByIdAsync(int id);

    Task<bool> ExisteSolapamiento(
        int profesionalId,
        DateTime inicio,
        DateTime fin
    );

    Task<IEnumerable<Turno>> GetAgendaProfesionalAsync(
        int profesionalId,
        DateTime? desde,
        DateTime? hasta
    );

    Task<IEnumerable<Turno>> GetByFecha(DateTime fecha);
}
