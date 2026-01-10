using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class BloqueoTiempoService : IBloqueoTiempoService
    {
        private readonly IBloqueoTiempoRepository _bloqueoRepository;
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly ITurnoRepository _turnoRepository;

        public BloqueoTiempoService(
            IBloqueoTiempoRepository bloqueoRepository,
            IProfesionalRepository profesionalRepository,
            ITurnoRepository turnoRepository)
        {
            _bloqueoRepository = bloqueoRepository;
            _profesionalRepository = profesionalRepository;
            _turnoRepository = turnoRepository;
        }

        public async Task<IEnumerable<BloqueoTiempoDto>> GetByProfesionalIdAsync(int profesionalId, DateTime desde, DateTime hasta)
        {
            var bloqueos = await _bloqueoRepository.GetByProfesionalIdAsync(profesionalId, desde, hasta);
            return bloqueos.Select(MapToDto);
        }

        public async Task<BloqueoTiempoDto> CreateAsync(int profesionalId, BloqueoTiempoCreateDto createDto)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(profesionalId) ?? throw new BusinessException("Profesional no encontrado.");

            if (createDto.FechaHoraInicio >= createDto.FechaHoraFin)
            {
                throw new BusinessException("La fecha de inicio debe ser menor que la fecha de fin.");
            }

            // Chequear solapamiento con turnos existentes
            var turnoSolapado = await _turnoRepository.ExisteSolapamiento(profesionalId, createDto.FechaHoraInicio, createDto.FechaHoraFin);
            if (turnoSolapado)
            {
                throw new BusinessException("El bloqueo de tiempo se solapa con un turno existente.");
            }

            // Chequear solapamiento con otros bloqueos
            var bloqueoSolapado = await _bloqueoRepository.ExisteSolapamiento(profesionalId, createDto.FechaHoraInicio, createDto.FechaHoraFin);
            if (bloqueoSolapado)
            {
                throw new BusinessException("El bloqueo de tiempo se solapa con otro bloqueo existente.");
            }

            var nuevoBloqueo = new BloqueoTiempo
            {
                ProfesionalId = profesionalId,
                FechaHoraInicio = createDto.FechaHoraInicio,
                FechaHoraFin = createDto.FechaHoraFin,
                Motivo = createDto.Motivo
            };

            await _bloqueoRepository.AddAsync(nuevoBloqueo);
            return MapToDto(nuevoBloqueo);
        }

        public async Task DeleteAsync(int id)
        {
            var bloqueo = await _bloqueoRepository.GetByIdAsync(id) ?? throw new NotFoundException("Bloqueo de tiempo no encontrado.");
            await _bloqueoRepository.DeleteAsync(bloqueo);
        }

        private static BloqueoTiempoDto MapToDto(BloqueoTiempo bloqueo)
        {
            return new BloqueoTiempoDto
            {
                Id = bloqueo.Id,
                ProfesionalId = bloqueo.ProfesionalId,
                FechaHoraInicio = bloqueo.FechaHoraInicio,
                FechaHoraFin = bloqueo.FechaHoraFin,
                Motivo = bloqueo.Motivo
            };
        }
    }
}
