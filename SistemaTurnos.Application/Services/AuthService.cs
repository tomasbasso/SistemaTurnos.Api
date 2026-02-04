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
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IPersonaService personaService,
            IProfesionalRepository profesionalRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _personaService = personaService;
            _profesionalRepository = profesionalRepository;
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

            // Check for lockout
            var maxFailedAttempts = int.Parse(_configuration["Security:MaxFailedLoginAttempts"] ?? "5");
            var lockoutMinutes = int.Parse(_configuration["Security:LockoutMinutes"] ?? "15");

            if (persona.LockoutEnd.HasValue && persona.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Cuenta bloqueada para {Email} hasta {LockoutEnd}", dto.Email, persona.LockoutEnd.Value);
                throw new BusinessException("Cuenta bloqueada. Intente más tarde");
            }

            var passwordValida = Verify(dto.Password, persona.PasswordHash);

            if (!persona.Activo || !passwordValida)
            {
                // Increment failed attempts and possibly lock the account
                persona.FailedLoginAttempts++;

                if (persona.FailedLoginAttempts >= maxFailedAttempts)
                {
                    persona.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    persona.FailedLoginAttempts = 0; // reset counter after lockout
                    _logger.LogWarning("Cuenta bloqueada por intentos fallidos para {Email} hasta {LockoutEnd}", dto.Email, persona.LockoutEnd.Value);
                }
                else
                {
                    _logger.LogWarning("Login fallido para {Email}. Intentos fallidos: {Attempts}", dto.Email, persona.FailedLoginAttempts);
                }

                await _personaService.UpdatePersonaAsync(persona);

                throw new BusinessException("Credenciales inválidas");
            }

            // Successful login: reset failed attempts and lockout
            if (persona.FailedLoginAttempts > 0 || persona.LockoutEnd.HasValue)
            {
                persona.FailedLoginAttempts = 0;
                persona.LockoutEnd = null;
                await _personaService.UpdatePersonaAsync(persona);
            }

            var userDto = await _personaService.GetByEmailAsync(dto.Email);
            var token = GenerateToken(persona);

            int? profesionalId = null;
            if (persona.Rol == Domain.Enums.Rol.Profesional)
            {
                 var profesional = await _profesionalRepository.GetByPersonaIdAsync(persona.Id);
                 if (profesional != null) profesionalId = profesional.Id;
            }

            return new AuthResponseDto
            {
                Token = token,
                User = userDto!,
                ProfesionalId = profesionalId
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

            // Use the configured JWT key; in Development, fall back to a non-secret dev key so local runs/tests don't crash.
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    jwtKey = "dev_insecure_jwt_key_for_development_only";
                }
                else
                {
                    throw new InvalidOperationException("JWT key is not configured. Set the environment variable 'Jwt__Key' or provide a valid key in configuration.");
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = 60;
            if (!int.TryParse(_configuration["Jwt:ExpireMinutes"], out expireMinutes))
            {
                _logger.LogWarning("Jwt:ExpireMinutes no configurado o inválido, usando {Default} minutos", expireMinutes);
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}