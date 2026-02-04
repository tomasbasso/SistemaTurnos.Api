using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaTurnos.Api.Middleware;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Infrastructure.Persistence;
using SistemaTurnos.Infrastructure.Repositories;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Global handlers for otherwise-unobserved exceptions so we can see what crashes the process and avoid silent shutdowns.
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    try
    {
        var ex = e.ExceptionObject as Exception;
        Console.Error.WriteLine($"Unhandled exception (AppDomain): {ex?.ToString() ?? e.ExceptionObject?.ToString()}");
    }
    catch { }
};

TaskScheduler.UnobservedTaskException += (s, e) =>
{
    try
    {
        Console.Error.WriteLine($"Unobserved task exception: {e.Exception}");
        e.SetObserved();
    }
    catch { }
};

// Security: require JWT key in non-development environments
if (!builder.Environment.IsDevelopment())
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("JWT key is not configured. Set it via environment variable 'Jwt__Key' or secret manager.");
    }
}

// Add services to the container.

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // Use a development fallback key only when running in Development (tests/local). In production, the key must be provided via secrets/env.
            // This prevents crashes in tests where the project does not set a real JWT secret.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Key"])
                    ? "dev_insecure_jwt_key_for_development_only"
                    : builder.Configuration["Jwt:Key"]
            ))
        };
    });

builder.Services.AddAuthorization();

// Rate limiting: protect login endpoint from brute-force attacks (per IP)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginPolicy", httpContext =>
    {
        // Partition by remote IP (or X-Forwarded-For when behind proxy)
        var key = httpContext.Connection.RemoteIpAddress?.ToString()
                  ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                  ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

builder.Services.AddValidatorsFromAssembly(
    typeof(PersonaCreateDtoValidator).Assembly
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// DbContext
builder.Services.AddDbContext<SistemaTurnosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioService, ServicioService>();

builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<TurnoService>();

builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();

builder.Services.AddScoped<IHorarioTrabajoRepository, HorarioTrabajoRepository>();
builder.Services.AddScoped<IHorarioTrabajoService, HorarioTrabajoService>();

builder.Services.AddScoped<IBloqueoTiempoRepository, BloqueoTiempoRepository>();
builder.Services.AddScoped<IBloqueoTiempoService, BloqueoTiempoService>();

builder.Services.AddScoped<IProfesionalServicioService, ProfesionalServicioService>();

builder.Services.AddScoped<IReporteService, ReporteService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IAuditService, SistemaTurnos.Infrastructure.Services.AuditService>();

builder.Services.AddScoped<INotaClinicaRepository, NotaClinicaRepository>();
builder.Services.AddScoped<IHistorialClinicoService, HistorialClinicoService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Temporary request logger to capture incoming requests and any unhandled exception details.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
    logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);
    try
    {
        await next();
        logger.LogInformation("Request finished {StatusCode} {Path}", context.Response.StatusCode, context.Request.Path);
    }
    catch (Exception ex)
    {
        // Log and return a JSON 500 response here to avoid unhandled exceptions bubbling to the host process and causing shutdown.
        logger.LogError(ex, "Unhandled exception in request {Path}", context.Request.Path);
        try
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            if (app.Environment.IsDevelopment())
            {
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stack = ex.StackTrace
                }));
            }
            else
            {
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { error = "Se produjo un error interno" }));
            }
        }
        catch (Exception writeEx)
        {
            logger.LogError(writeEx, "Failed writing error response");
        }
        // Do not rethrow, we handled the exception and returned an error to the client.
    }
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

// Global rate limiter middleware (enables policies defined above)
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

// Log host lifecycle events to help debug unexpected shutdowns.
var lifetime = app.Lifetime;
lifetime.ApplicationStopping.Register(() => Console.Error.WriteLine($"ApplicationStopping invoked at {DateTime.UtcNow:O}"));
lifetime.ApplicationStopped.Register(() => Console.Error.WriteLine($"ApplicationStopped invoked at {DateTime.UtcNow:O}"));

app.Run();
