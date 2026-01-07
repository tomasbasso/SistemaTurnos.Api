using SistemaTurnos.Domain.Enums;

public class Turno
{
    public int Id { get; private set; }

    public int PersonaId { get; private set; }
    public int ProfesionalId { get; private set; }
    public int ServicioId { get; private set; }

    public DateTime FechaHoraInicio { get; private set; }
    public DateTime FechaHoraFin { get; private set; }

    public EstadoTurno Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    protected Turno() { }

    public Turno(
        int personaId,
        int profesionalId,
        int servicioId,
        DateTime inicio,
        int duracionMinutos)
    {
        PersonaId = personaId;
        ProfesionalId = profesionalId;
        ServicioId = servicioId;

        FechaHoraInicio = inicio;
        FechaHoraFin = inicio.AddMinutes(duracionMinutos);

        Estado = EstadoTurno.Activo;
        FechaCreacion = DateTime.Now;
    }

    public void Cancelar()
    {
        if (Estado == EstadoTurno.Finalizado)
            throw new InvalidOperationException(
                "No se puede cancelar un turno finalizado"
            );

        Estado = EstadoTurno.Cancelado;
    }
    public void Finalizar()
    {
        if (Estado != EstadoTurno.Activo)
            throw new InvalidOperationException(
                "Solo se pueden finalizar turnos activos"
            );

        if (DateTime.Now < FechaHoraInicio)
            throw new InvalidOperationException(
                "No se puede finalizar un turno que aún no comenzó"
            );

        Estado = EstadoTurno.Finalizado;
    }

}
