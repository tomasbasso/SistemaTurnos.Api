using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaTurnos.Domain.Entities;
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


            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<SistemaTurnosDbContext>();

            db.Database.EnsureCreated();


            db.Personas.Add(new Persona(
                nombre: "Juan",
                dni: "12345678",
                email: "juan@test.com"
            ));

            db.Profesionales.Add(new Profesional(
                nombre: "Dr. Smith",
                matricula: "MAT-001"
            ));

            db.Servicios.Add(new Servicio(
                nombre: "Consulta",
                descripcion: "Consulta general",
                duracionMinutos: 30,
                precio: 5000
            ));

            db.Turnos.Add(new Turno(
                personaId: 1,
                profesionalId: 1,
                servicioId: 1,
                inicio: DateTime.Now.AddHours(-1),
                duracionMinutos: 30
            ));

            db.SaveChanges();
        });
    }
}
