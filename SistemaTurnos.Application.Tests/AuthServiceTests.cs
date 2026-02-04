using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Domain.Exceptions;

public class AuthServiceTests
{
    private readonly Mock<IPersonaService> _personaService = new();
    private readonly Mock<IProfesionalRepository> _profRepo = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();

    private AuthService CreateService(IConfiguration config)
    {
        return new AuthService(_personaService.Object, _profRepo.Object, config, _logger.Object);
    }

    [Fact]
    public async Task Login_WrongPassword_IncrementsFailedAttemptsAndLocks()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>{
            { "Security:MaxFailedLoginAttempts", "2" },
            { "Security:LockoutMinutes", "5" },
            { "Jwt:Key", "0123456789ABCDEF0123456789ABCDEF" },
            { "Jwt:Issuer", "s" },
            { "Jwt:Audience", "s" },
            { "Jwt:ExpireMinutes", "60" }
        }).Build();

        var persona = new Persona("Test", "123", "t@test.com", BCrypt.Net.BCrypt.HashPassword("correct"), Rol.Cliente);
        persona.FailedLoginAttempts = 0;

        _personaService.Setup(p => p.GetPersonaByEmailAsync(It.IsAny<string>())).ReturnsAsync(persona);
        _personaService.Setup(p => p.UpdatePersonaAsync(It.IsAny<Persona>())).Returns(Task.CompletedTask).Verifiable();

        var svc = CreateService(cfg);

        // First wrong attempt
        await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginDto { Email = persona.Email, Password = "wrong" }));
        Assert.Equal(1, persona.FailedLoginAttempts);
        _personaService.Verify(p => p.UpdatePersonaAsync(It.IsAny<Persona>()), Times.Once);

        // Second wrong attempt -> lockout triggers
        await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginDto { Email = persona.Email, Password = "wrong" }));
        Assert.Equal(0, persona.FailedLoginAttempts); // reset after lock
        Assert.NotNull(persona.LockoutEnd);
        _personaService.Verify(p => p.UpdatePersonaAsync(It.IsAny<Persona>()), Times.Exactly(2));

        // Attempt during lockout should throw
        await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginDto { Email = persona.Email, Password = "correct" }));
    }

    [Fact]
    public async Task Login_Success_ResetsFailedAttemptsAndLockout()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>{
            { "Security:MaxFailedLoginAttempts", "3" },
            { "Security:LockoutMinutes", "1" },
            { "Jwt:Key", "0123456789ABCDEF0123456789ABCDEF" },
            { "Jwt:Issuer", "s" },
            { "Jwt:Audience", "s" },
            { "Jwt:ExpireMinutes", "60" }
        }).Build();

        var persona = new Persona("Test", "123", "t@test.com", BCrypt.Net.BCrypt.HashPassword("correct"), Rol.Cliente);
        persona.FailedLoginAttempts = 2;
        persona.LockoutEnd = null;

        _personaService.Setup(p => p.GetPersonaByEmailAsync(It.IsAny<string>())).ReturnsAsync(persona);
        _personaService.Setup(p => p.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(new SistemaTurnos.Application.DTOs.PersonaDto { Email = persona.Email });
        _personaService.Setup(p => p.UpdatePersonaAsync(It.IsAny<Persona>())).Returns(Task.CompletedTask).Verifiable();

        var svc = CreateService(cfg);

        var result = await svc.LoginAsync(new LoginDto { Email = persona.Email, Password = "correct" });

        Assert.NotNull(result);
        Assert.False(persona.LockoutEnd.HasValue);
        Assert.Equal(0, persona.FailedLoginAttempts);
        _personaService.Verify(p => p.UpdatePersonaAsync(It.IsAny<Persona>()), Times.Once);
    }
}