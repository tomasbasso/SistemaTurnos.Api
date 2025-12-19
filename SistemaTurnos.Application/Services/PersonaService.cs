using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;

namespace SistemaTurnos.Application.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly IPersonaRepository _repository;

        public PersonaService(IPersonaRepository repository)
        {
            _repository = repository;
        }

        // CREATE
        public async Task<PersonaDto> CrearAsync(PersonaCreateDto dto)
        {
            var persona = new Persona(dto.Nombre, dto.Dni, dto.Email);
            await _repository.AddAsync(persona);
            return MapToDto(persona);
        }

        // UPDATE
        public async Task ActualizarAsync(int id, PersonaCreateDto dto)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            persona.Nombre = dto.Nombre;
            persona.Dni = dto.Dni;
            persona.Email = dto.Email;

            await _repository.SaveChangesAsync();
        }

        // DELETE (lógico)
        public async Task EliminarAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            persona.Activo = false;
            await _repository.SaveChangesAsync();
        }

        // REACTIVAR
        public async Task ReactivarAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Persona no encontrada");

            persona.Activo = true;
            await _repository.SaveChangesAsync();
        }

        // GET BY ID
        public async Task<PersonaDto?> GetByIdAsync(int id)
        {
            var persona = await _repository.GetByIdAsync(id);

            return persona == null || !persona.Activo
                ? null
                : MapToDto(persona);
        }

        // GET ALL / SEARCH
        public async Task<List<PersonaDto>> GetAllAsync(string? busqueda)
        {
            var personas = await _repository.GetAllAsync(busqueda);
            return personas.Select(MapToDto).ToList();
        }

        // =========================
        // Mapping
        // =========================
        private static PersonaDto MapToDto(Persona p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Dni = p.Dni,
            Email = p.Email
        };
    }
}
