using Moq;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using Xunit;
using System.Threading.Tasks;

public class PersonaServiceTests
{
    private readonly Mock<IPersonaRepository> _repository = new();
    private readonly Mock<IProfesionalRepository> _profesionalRepository = new();
    private readonly Mock<ITurnoRepository> _turnoRepository = new();
    private readonly PersonaService _service;

    public PersonaServiceTests()
    {
        _service = new PersonaService(_repository.Object, _profesionalRepository.Object, _turnoRepository.Object);
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_RetornaDto()
    {
        // Arrange
        var dto = new PersonaCreateDto
        {
            Nombre = "Juan Perez",
            Dni = "12345678",
            Email = "juan@example.com",
            Password = "password123",
            Rol = Rol.Cliente
        };

        _repository.Setup(r => r.ExisteDniAsync(dto.Dni, It.IsAny<int?>())).ReturnsAsync(false);
        _repository.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Persona?)null);
        _repository.Setup(r => r.AddAsync(It.IsAny<Persona>()));
        _repository.Setup(r => r.SaveChangesAsync());

        // Act
        var result = await _service.CrearAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Nombre, result.Nombre);
        Assert.Equal(dto.Dni, result.Dni);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.Rol, result.Rol);
    }

    [Fact]
    public async Task CrearAsync_ConDniExistente_LanzaExcepcion()
    {
        // Arrange
        var dto = new PersonaCreateDto
        {
            Nombre = "Juan Perez",
            Dni = "12345678",
            Email = "juan@example.com",
            Password = "password123",
            Rol = Rol.Cliente
        };

        _repository.Setup(r => r.ExisteDniAsync(dto.Dni, It.IsAny<int?>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CrearAsync(dto));
    }

    [Fact]
    public async Task CrearAsync_ConEmailExistente_LanzaExcepcion()
    {
        // Arrange
        var dto = new PersonaCreateDto
        {
            Nombre = "Juan Perez",
            Dni = "12345678",
            Email = "juan@example.com",
            Password = "password123",
            Rol = Rol.Cliente
        };

        _repository.Setup(r => r.ExisteDniAsync(dto.Dni, It.IsAny<int?>())).ReturnsAsync(false);
        _repository.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Persona("Otro", "87654321", "juan@example.com", "hash", Rol.Cliente));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CrearAsync(dto));
    }
}