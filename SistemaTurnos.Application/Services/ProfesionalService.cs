using SistemaTurnos.Application.DTOs;
﻿using SistemaTurnos.Application.DTOs.Common;
﻿using SistemaTurnos.Application.Interfaces.Repositories;
﻿using SistemaTurnos.Application.Interfaces.Services;
﻿using SistemaTurnos.Domain.Entities;
﻿using SistemaTurnos.Domain.Enums;
﻿using SistemaTurnos.Domain.Exceptions;
﻿using System.Linq;
﻿using System.Threading.Tasks;
﻿
﻿namespace SistemaTurnos.Application.Services
﻿{
﻿    public class ProfesionalService : IProfesionalService
﻿    {
﻿        private readonly IProfesionalRepository _profesionalRepository;
﻿        private readonly IPersonaRepository _personaRepository;
﻿
﻿        public ProfesionalService(IProfesionalRepository profesionalRepository, IPersonaRepository personaRepository)
﻿        {
﻿            _profesionalRepository = profesionalRepository;
﻿            _personaRepository = personaRepository;
﻿        }
﻿
﻿        public async Task<ProfesionalDto> CrearAsync(ProfesionalCreateDto dto)
﻿        {
﻿            // Validar que la persona exista
﻿            var persona = await _personaRepository.GetByIdAsync(dto.PersonaId) 
﻿                ?? throw new BusinessException("La persona especificada no existe.");
﻿
﻿            // Validar que la persona tenga el rol de profesional
﻿            if (persona.Rol != Rol.Profesional)
﻿                throw new BusinessException("La persona debe tener el rol de 'Profesional'.");
﻿
﻿            // Validar que la persona no esté ya asignada a otro profesional
﻿            if (await _profesionalRepository.ExisteAsignacion(dto.PersonaId))
﻿                throw new BusinessException("Esta persona ya está asignada a un perfil profesional.");
﻿
﻿            // Validar que la matrícula no exista
﻿            if (await _profesionalRepository.ExisteMatriculaAsync(dto.Matricula))
﻿                throw new BusinessException("La matrícula ya existe.");
﻿
﻿            var profesional = new Profesional(dto.PersonaId, dto.Matricula);
﻿
﻿            await _profesionalRepository.AddAsync(profesional);
﻿
﻿            // Cargar la persona para el mapeo
﻿            profesional.Persona = persona;
﻿
﻿            return MapToDto(profesional);
﻿        }
﻿
﻿        public async Task ActualizarAsync(int id, ProfesionalUpdateDto dto)
﻿        {
﻿            var profesional = await _profesionalRepository.GetByIdAsync(id)
﻿                ?? throw new BusinessException("Profesional no encontrado");
﻿
﻿            if (dto.Matricula != null &&
﻿                dto.Matricula != profesional.Matricula &&
﻿                await _profesionalRepository.ExisteMatriculaAsync(dto.Matricula, id))
﻿                throw new BusinessException("La matrícula ya está en uso");
﻿            
﻿            if (dto.Matricula != null)
﻿                profesional.Matricula = dto.Matricula;
﻿
﻿            await _profesionalRepository.SaveChangesAsync();
﻿        }
﻿
﻿        public async Task EliminarAsync(int id)
﻿        {
﻿            var profesional = await _profesionalRepository.GetByIdAsync(id)
﻿                ?? throw new BusinessException("Profesional no encontrado");
﻿
﻿            profesional.Activo = false;
﻿            await _profesionalRepository.SaveChangesAsync();
﻿        }
﻿
﻿        public async Task ReactivarAsync(int id)
﻿        {
﻿            var profesional = await _profesionalRepository.GetByIdAsync(id)
﻿                ?? throw new BusinessException("Profesional no encontrado");
﻿
﻿            profesional.Activo = true;
﻿            await _profesionalRepository.SaveChangesAsync();
﻿        }
﻿
﻿        public async Task<ProfesionalDto?> GetByIdAsync(int id)
﻿        {
﻿            var profesional = await _profesionalRepository.GetByIdAsync(id);
﻿
﻿            return profesional == null || !profesional.Activo
﻿                ? null
﻿                : MapToDto(profesional);
﻿        }
﻿
﻿        public async Task<PagedResultDto<ProfesionalDto>> GetPagedAsync(
﻿            string? busqueda,
﻿            int page,
﻿            int pageSize,
﻿            string? sortBy,
﻿            string? sortDir)
﻿        {
﻿            var (items, total) = await _profesionalRepository.GetPagedAsync(
﻿                busqueda, page, pageSize, sortBy, sortDir);
﻿
﻿            return new PagedResultDto<ProfesionalDto>
﻿            {
﻿                Page = page,
﻿                PageSize = pageSize,
﻿                TotalItems = total,
﻿                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
﻿                Items = items.Select(MapToDto).ToList()
﻿            };
﻿        }
﻿
﻿        private static ProfesionalDto MapToDto(Profesional p) => new()
﻿        {
﻿            Id = p.Id,
﻿            Nombre = p.Persona?.Nombre ?? "N/A", // Obtener nombre de la Persona
﻿            Matricula = p.Matricula
﻿        };
﻿    }
﻿}
﻿
