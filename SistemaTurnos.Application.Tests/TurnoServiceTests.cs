using Moq;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class TurnoServiceTests
{
    private readonly Mock<ITurnoRepository> _turnos = new();
    private readonly Mock<IPersonaRepository> _personas = new();
    private readonly Mock<IProfesionalRepository> _profesionales = new();
    private readonly Mock<IServicioRepository> _servicios = new();
    private readonly Mock<IHorarioTrabajoRepository> _horarios = new();
    private readonly Mock<IBloqueoTiempoRepository> _bloqueos = new();

    private TurnoService CrearService()
    {
        return new TurnoService(
            _turnos.Object,
            _personas.Object,
            _profesionales.Object,
            _servicios.Object,
            _horarios.Object,
            _bloqueos.Object
        );
    }

    private TurnoCreateDto CrearDtoValido()
    {
        return new TurnoCreateDto
        {
            ProfesionalId = 1,
            ServicioId = 1,
            FechaHoraInicio = DateTime.Now.AddHours(1)
        };
    }

    private Persona CrearPersonaValida()
    {
        return new Persona("Juan Perez", "12345678", "juan@test.com", "hash", Rol.Cliente);
    }

    private void SetupDatosValidos()
    {
        _personas.Setup(p => p.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CrearPersonaValida());

        _profesionales.Setup(p => p.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Profesional(1, "MAT-123"));

        _servicios.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Servicio("Consulta", "General", 30, 5000));

        _turnos.Setup(t => t.ExisteSolapamiento(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        _horarios.Setup(h => h.GetByProfesionalIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<HorarioTrabajo> { 
                new HorarioTrabajo { DiaSemana = DateTime.Now.AddHours(1).DayOfWeek, HoraInicio = TimeOnly.FromDateTime(DateTime.Now), HoraFin = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)) }
            });
    }

    // --------------------
    // CREAR
    // --------------------

    [Fact]
    public async Task Crear_TurnoEnElPasado_LanzaBusinessException()
    {
        var service = CrearService();
        var dto = CrearDtoValido();
        dto.FechaHoraInicio = DateTime.Now.AddHours(-1);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CrearAsync(dto, 1)
        );
    }

    [Fact]
    public async Task Crear_PersonaInexistente_LanzaBusinessException()
    {
        var service = CrearService();
        var dto = CrearDtoValido();

        _personas.Setup(p => p.GetByIdAsync(1))
            .ReturnsAsync((Persona?)null);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CrearAsync(dto, 1)
        );
    }

    [Fact]
    public async Task Crear_ProfesionalInexistente_LanzaBusinessException()
    {
        var service = CrearService();
        var dto = CrearDtoValido();

        _personas.Setup(p => p.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CrearPersonaValida());

        _profesionales.Setup(p => p.GetByIdAsync(1))
            .ReturnsAsync((Profesional?)null);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CrearAsync(dto, 1)
        );
    }

    [Fact]
    public async Task Crear_ServicioInactivo_LanzaBusinessException()
    {
        var service = CrearService();
        var dto = CrearDtoValido();

        _personas.Setup(p => p.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(CrearPersonaValida());

        _profesionales.Setup(p => p.GetByIdAsync(1))
            .ReturnsAsync(new Profesional(1, "MAT-123"));

        var servicio = new Servicio("Consulta", "General", 30, 5000);
        servicio.Activo = false;

        _servicios.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(servicio);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CrearAsync(dto, 1)
        );
    }

    [Fact]
    public async Task Crear_TurnoSolapado_LanzaBusinessException()
    {
        var service = CrearService();
        var dto = CrearDtoValido();

        SetupDatosValidos();

        _turnos.Setup(t => t.ExisteSolapamiento(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CrearAsync(dto, 1)
        );
    }

    [Fact]
    public async Task Crear_TurnoValido_CreaCorrectamente()
    {
        var service = CrearService();
        var dto = CrearDtoValido();

        SetupDatosValidos();

        var turno = await service.CrearAsync(dto, 1);

        Assert.NotNull(turno);
        Assert.Equal(EstadoTurno.Activo, turno.Estado);
        Assert.True(turno.FechaHoraFin > turno.FechaHoraInicio);
    }

    // --------------------
    // FINALIZAR
    // --------------------

    [Fact]
    public async Task Finalizar_TurnoActivo_FinalizaCorrectamente()
    {
        var service = CrearService();

        var turno = new Turno(
            personaId: 1,
            profesionalId: 1,
            servicioId: 1,
            inicio: DateTime.Now.AddMinutes(-40),
            duracionMinutos: 30
        );

        _turnos.Setup(t => t.GetByIdAsync(1))
            .ReturnsAsync(turno);

        await service.FinalizarAsync(1);

        Assert.Equal(EstadoTurno.Finalizado, turno.Estado);
    }

    [Fact]
    public async Task Finalizar_TurnoCancelado_LanzaExcepcion()
    {
        var service = CrearService();

        var turno = new Turno(
            1, 1, 1,
            DateTime.Now.AddMinutes(-40),
            30
        );

        turno.Cancelar();

        _turnos.Setup(t => t.GetByIdAsync(1))
            .ReturnsAsync(turno);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.FinalizarAsync(1)
        );
    }

    // --------------------
    // AGENDA PROFESIONAL
    // --------------------

    [Fact]
    public async Task ObtenerAgendaProfesional_ProfesionalInexistente_LanzaBusinessException()
    {
        var service = CrearService();

        _profesionales.Setup(p => p.GetByIdAsync(1))
            .ReturnsAsync((Profesional?)null);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.ObtenerAgendaProfesionalAsync(1, DateTime.Now, DateTime.Now.AddDays(1))
        );
    }

    [Fact]
    public async Task ObtenerAgendaProfesional_RangoInvalido_LanzaBusinessException()
    {
        var service = CrearService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.ObtenerAgendaProfesionalAsync(1, DateTime.Now.AddDays(1), DateTime.Now)
        );
    }

    [Fact]
    public async Task ObtenerAgendaProfesional_Valido_RetornaAgenda()
    {
        var service = CrearService();

        var profesional = new Profesional(1, "MAT-123") { Activo = true };
        _profesionales.Setup(p => p.GetByIdAsync(1))
            .ReturnsAsync(profesional);

        var turnos = new List<Turno>
        {
            new Turno(1, 1, 1, DateTime.Now.AddHours(1), 30),
            new Turno(2, 1, 1, DateTime.Now.AddHours(2), 30)
        };

        turnos[0].Persona = new Persona("Juan Perez", "12345678", "juan@test.com", "hash", Rol.Cliente);
        turnos[0].Servicio = new Servicio("Consulta", "General", 30, 5000);
        turnos[1].Persona = new Persona("Ana Gomez", "87654321", "ana@test.com", "hash", Rol.Cliente);
        turnos[1].Servicio = new Servicio("Consulta", "General", 30, 5000);

        _turnos.Setup(t => t.GetAgendaProfesionalAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(turnos);

        var result = await service.ObtenerAgendaProfesionalAsync(1, DateTime.Now, DateTime.Now.AddDays(1));

        Assert.Equal(2, result.Count());
        Assert.Equal("Juan Perez", result.First().PersonaNombre);
    }
}
