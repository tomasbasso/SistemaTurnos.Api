using static BCrypt.Net.BCrypt;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;

namespace SistemaTurnos.Application.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly IPersonaRepository _repository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly ITurnoRepository _turnoRepository;

        public PersonaService(IPersonaRepository repository, IProfesionalRepository profesionalRepository, ITurnoRepository turnoRepository)
        {
            _repository = repository;
            _profesionalRepository = profesionalRepository;
            _turnoRepository = turnoRepository;
        }
        public async Task<PagedResultDto<PersonaDto>> GetPagedAsync(
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var (items, total) =
                await _repository.GetPagedAsync(
                    busqueda,
                    page,
                    pageSize,
                    sortBy,
                    sortDir);

            return new PagedResultDto<PersonaDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items.Select(MapToDto).ToList()
            };
        }


        // =========================
        // CREATE
        // =========================
        public async Task<PersonaDto> CrearAsync(PersonaCreateDto dto)
        {
            if (await _repository.ExisteDniAsync(dto.Dni))
                throw new BusinessException($"El DNI {dto.Dni} ya existe");

            if (await _repository.GetByEmailAsync(dto.Email) != null)
                throw new BusinessException($"El email {dto.Email} ya existe");

            var passwordHash = HashPassword(dto.Password);
            var persona = new Persona(dto.Nombre, dto.Dni, dto.Email, passwordHash, dto.Rol);

            await _repository.AddAsync(persona);
            await _repository.SaveChangesAsync();

            // Si es Profesional, crear registro en tabla Profesionales
            if (persona.Rol == Domain.Enums.Rol.Profesional)
            {
                var profesional = new Profesional(persona.Id, "PENDIENTE"); // Matricula por defecto
                await _profesionalRepository.AddAsync(profesional);
                await _profesionalRepository.SaveChangesAsync();
            }

            return MapToDto(persona);
        }

        // =========================
        // UPDATE
        // =========================
        public async Task ActualizarAsync(int id, PersonaUpdateDto dto)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            if (dto.Nombre != null)
                persona.Nombre = dto.Nombre;

            if (dto.Dni != null &&
            await _repository.ExisteDniAsync(dto.Dni, id))
            {
                throw new BusinessException($"El DNI {dto.Dni} ya existe");
            }


            if (dto.Email != null)
                persona.Email = dto.Email;

            if (dto.Rol.HasValue)
                persona.Rol = dto.Rol.Value;

            await _repository.SaveChangesAsync();
        }

        // =========================
        // DELETE (lógico)
        // =========================
        public async Task EliminarAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            if (!persona.Activo)
                throw new BusinessException("La persona ya está desactivada");

            persona.Activo = false;
            await _repository.SaveChangesAsync();
        }

        // =========================
        // REACTIVAR
        // =========================
        public async Task ReactivarAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            if (persona.Activo)
                throw new BusinessException("La persona ya está activa");

            persona.Activo = true;
            await _repository.SaveChangesAsync();
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<PersonaDto?> GetByIdAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id);

            return persona == null || !persona.Activo
                ? null
                : MapToDto(persona);
        }

        // =========================
        // GET ALL / SEARCH
        // =========================
        public async Task<List<PersonaDto>> GetAllAsync(string? busqueda)
        {
            var personas = await _repository.GetAllAsync(busqueda);
            return personas.Select(MapToDto).ToList();
        }

        // =========================
        // GET BY EMAIL
        // =========================
        public async Task<PersonaDto?> GetByEmailAsync(string email)
        {
            var persona = await _repository.GetByEmailAsync(email);

            return persona == null || !persona.Activo
                ? null
                : MapToDto(persona);
        }

        // =========================
        // GET PERSONA BY EMAIL (ENTITY)
        // =========================
        public async Task<Persona?> GetPersonaByEmailAsync(string email)
        {
            return await _repository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<PersonaDto>> GetPacientesByProfesionalAsync(int profesionalId)
        {
            var pacientes = await _turnoRepository.GetPacientesByProfesionalAsync(profesionalId);
            return pacientes.Select(MapToDto).ToList();
        }

        // =========================
        // Mapping
        // =========================
        private static PersonaDto MapToDto(Persona p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Dni = p.Dni,
            Email = p.Email,
            Rol = p.Rol,
            ProfesionalId = p.Profesional?.Id,
            ProfesionalActivo = p.Profesional?.Activo ?? false
        };
    }
}
