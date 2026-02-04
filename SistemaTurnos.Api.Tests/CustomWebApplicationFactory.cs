using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Infrastructure.Persistence;
using System.Linq;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remover el DbContext real
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SistemaTurnosDbContext>)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            // Registrar DbContext InMemory
            services.AddDbContext<SistemaTurnosDbContext>(options =>
            {
                options.UseInMemoryDatabase("SistemaTurnos_TestDb");
            });

            // Test authentication: auto-authenticate requests using TestAuthHandler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                "Test", _ => { });


            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<SistemaTurnosDbContext>();

            db.Database.EnsureCreated();


            // Seed: persona
            var persona = new Persona(
                nombre: "Juan",
                dni: "12345678",
                email: "juan@test.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("password"),
                rol: Rol.Cliente
            );
            db.Personas.Add(persona);
            db.SaveChanges();

            // Seed: servicio
            var servicio = new Servicio(
                nombre: "Consulta",
                descripcion: "Consulta general",
                duracionMinutos: 30,
                precio: 5000
            );
            db.Servicios.Add(servicio);

            // Seed: profesional (vincular servicio)
            var profesional = new Profesional(personaId: persona.Id, matricula: "MAT-001");
            profesional.Servicios.Add(servicio);
            db.Profesionales.Add(profesional);

            // Seed: horario del profesional (permite reservar) - full day to avoid flaky tests
            db.HorariosTrabajo.Add(new HorarioTrabajo {
                Profesional = profesional,
                DiaSemana = DateTime.Now.DayOfWeek,
                HoraInicio = TimeOnly.Parse("00:00"),
                HoraFin = TimeOnly.Parse("23:59")
            });

            // Seed: turno histórico (in the past, avoid conflicts with today's slots)
            db.Turnos.Add(new Turno(
                personaId: persona.Id,
                profesionalId: profesional.Id,
                servicioId: servicio.Id,
                inicio: DateTime.Now.AddDays(-1),
                duracionMinutos: 30
            ));

            db.SaveChanges();
        });
    }
}
