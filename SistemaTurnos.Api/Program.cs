using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Api.Middleware;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Infrastructure.Persistence;
//using SistemaTurnos.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssembly(
    typeof(PersonaCreateDtoValidator).Assembly
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<SistemaTurnosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<TurnoService>();

builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
