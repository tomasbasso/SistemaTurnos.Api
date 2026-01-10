using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static BCrypt.Net.BCrypt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Domain.Exceptions;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SistemaTurnos.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPersonaService _personaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IPersonaService personaService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _personaService = personaService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Intento de login para el email: {Email}", dto.Email);
            var persona = await _personaService.GetPersonaByEmailAsync(dto.Email);

            if (persona == null)
            {
                _logger.LogWarning("Login fallido: Usuario no encontrado para el email: {Email}", dto.Email);
                throw new BusinessException("Credenciales inválidas");
            }

            _logger.LogInformation("Usuario encontrado. Hash de la BD: {PasswordHash}", persona.PasswordHash);

            var passwordValida = Verify(dto.Password, persona.PasswordHash);
            _logger.LogInformation("Resultado de la verificación de la contraseña: {PasswordValida}", passwordValida);

            if (!persona.Activo || !passwordValida)
            {
                _logger.LogWarning("Login fallido para {Email}. Activo: {Activo}, Contraseña Válida: {PasswordValida}", dto.Email, persona.Activo, passwordValida);
                throw new BusinessException("Credenciales inválidas");
            }

            var userDto = await _personaService.GetByEmailAsync(dto.Email);
            var token = GenerateToken(persona);

            return new AuthResponseDto
            {
                Token = token,
                User = userDto!
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(PersonaCreateDto dto)
        {
            var personaDto = await _personaService.CrearAsync(dto);
            var persona = await _personaService.GetPersonaByEmailAsync(dto.Email);
            if (persona == null)
            {
                throw new BusinessException("Error al registrar usuario");
            }

            var token = GenerateToken(persona);

            return new AuthResponseDto
            {
                Token = token,
                User = personaDto
            };
        }

        private string GenerateToken(Persona persona)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, persona.Id.ToString()),
                new Claim(ClaimTypes.Email, persona.Email),
                new Claim(ClaimTypes.Role, persona.Rol.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}