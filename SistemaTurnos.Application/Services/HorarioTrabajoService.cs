using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class HorarioTrabajoService : IHorarioTrabajoService
    {
        private readonly IHorarioTrabajoRepository _horarioRepository;
        private readonly IProfesionalRepository _profesionalRepository;

        public HorarioTrabajoService(IHorarioTrabajoRepository horarioRepository, IProfesionalRepository profesionalRepository)
        {
            _horarioRepository = horarioRepository;
            _profesionalRepository = profesionalRepository;
        }

        public async Task<IEnumerable<HorarioTrabajoDto>> GetByProfesionalIdAsync(int profesionalId)
        {
            var horarios = await _horarioRepository.GetByProfesionalIdAsync(profesionalId);
            return horarios.Select(MapToDto);
        }

        public async Task<HorarioTrabajoDto> CreateAsync(int profesionalId, HorarioTrabajoCreateDto createDto)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(profesionalId) ?? throw new BusinessException("Profesional no encontrado.");

            var horaInicio = TimeOnly.Parse(createDto.HoraInicio);
            var horaFin = TimeOnly.Parse(createDto.HoraFin);

            if (horaInicio >= horaFin)
            {
                throw new BusinessException("La hora de inicio debe ser menor que la hora de fin.");
            }

            // Chequear solapamiento con horarios existentes
            var horariosExistentes = await _horarioRepository.GetByProfesionalIdAsync(profesionalId);
            var solapado = horariosExistentes.Any(h =>
                h.DiaSemana == createDto.DiaSemana &&
                horaInicio < h.HoraFin &&
                horaFin > h.HoraInicio);

            if (solapado)
            {
                throw new BusinessException("El horario se solapa con un horario existente.");
            }

            var nuevoHorario = new HorarioTrabajo
            {
                ProfesionalId = profesionalId,
                DiaSemana = createDto.DiaSemana,
                HoraInicio = horaInicio,
                HoraFin = horaFin
            };

            await _horarioRepository.AddAsync(nuevoHorario);
            return MapToDto(nuevoHorario);
        }

        public async Task UpdateAsync(int id, HorarioTrabajoCreateDto updateDto)
        {
            var horario = await _horarioRepository.GetByIdAsync(id) ?? throw new NotFoundException("Horario no encontrado.");

            var horaInicio = TimeOnly.Parse(updateDto.HoraInicio);
            var horaFin = TimeOnly.Parse(updateDto.HoraFin);

            if (horaInicio >= horaFin)
            {
                throw new BusinessException("La hora de inicio debe ser menor que la hora de fin.");
            }

            horario.DiaSemana = updateDto.DiaSemana;
            horario.HoraInicio = horaInicio;
            horario.HoraFin = horaFin;

            await _horarioRepository.UpdateAsync(horario);
        }

        public async Task DeleteAsync(int id)
        {
            var horario = await _horarioRepository.GetByIdAsync(id) ?? throw new NotFoundException("Horario no encontrado.");
            await _horarioRepository.DeleteAsync(horario);
        }

        private static HorarioTrabajoDto MapToDto(HorarioTrabajo horario)
        {
            return new HorarioTrabajoDto
            {
                Id = horario.Id,
                ProfesionalId = horario.ProfesionalId,
                DiaSemana = horario.DiaSemana,
                HoraInicio = horario.HoraInicio.ToString("HH:mm"),
                HoraFin = horario.HoraFin.ToString("HH:mm"),
                Activo = horario.Activo
            };
        }
    }
}
