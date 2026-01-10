using SistemaTurnos.Application.DTOs;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(PersonaCreateDto dto);
    }
}