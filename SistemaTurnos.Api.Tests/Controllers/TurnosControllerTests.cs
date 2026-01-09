using System.Net;
using System.Net.Http.Json;
using Xunit;
using SistemaTurnos.Application.DTOs;

public class TurnosControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TurnosControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    [Fact]
    public async Task CrearTurno_Valido_RetornaOk()
    {
        var dto = new TurnoCreateDto
        {
            PersonaId = 1,
            ProfesionalId = 1,
            ServicioId = 1,
            FechaHoraInicio = DateTime.Now.AddHours(1)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/turnos",
            dto
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CrearTurno_EnElPasado_Retorna409()
    {
        var dto = new TurnoCreateDto
        {
            PersonaId = 1,
            ProfesionalId = 1,
            ServicioId = 1,
            FechaHoraInicio = DateTime.Now.AddHours(-2)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/turnos",
            dto
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    [Fact]
    public async Task CancelarTurno_Existente_Retorna204()
    {
        var dto = new TurnoCreateDto
        {
            PersonaId = 1,
            ProfesionalId = 1,
            ServicioId = 1,
            FechaHoraInicio = DateTime.Now.AddHours(1)
        };

        var crear = await _client.PostAsJsonAsync("/api/turnos", dto);
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var turno = await crear.Content.ReadFromJsonAsync<TurnoDto>();

        var response = await _client.PutAsync(
            $"/api/turnos/{turno!.Id}/cancelar",
            null
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    [Fact]
    public async Task AgendaProfesional_ProfesionalInexistente_Retorna409()
    {
        var desde = DateTime.Now;
        var hasta = DateTime.Now.AddDays(7);

        var response = await _client.GetAsync(
            $"/api/turnos/profesionales/999/agenda?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AgendaProfesional_RangoInvalido_Retorna409()
    {
        var desde = DateTime.Now.AddDays(7);
        var hasta = DateTime.Now;

        var response = await _client.GetAsync(
            $"/api/turnos/profesionales/1/agenda?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AgendaProfesional_Valida_RetornaOk()
    {
        var desde = DateTime.Now;
        var hasta = DateTime.Now.AddDays(7);

        var response = await _client.GetAsync(
            $"/api/turnos/profesionales/1/agenda?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
