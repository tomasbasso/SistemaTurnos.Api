using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class ProfesionalServicioService : IProfesionalServicioService
    {
        private readonly IProfesionalRepository _profesionalRepository;
        private readonly IServicioRepository _servicioRepository;

        public ProfesionalServicioService(IProfesionalRepository profesionalRepository, IServicioRepository servicioRepository)
        {
            _profesionalRepository = profesionalRepository;
            _servicioRepository = servicioRepository;
        }

        public async Task<IEnumerable<ServicioDto>> GetServiciosByProfesionalAsync(int profesionalId)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(profesionalId);
            if (profesional == null)
            {
                throw new NotFoundException("Profesional no encontrado.");
            }

         
            return profesional.Servicios.Select(s => new ServicioDto {
                Id = s.Id,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                DuracionMinutos = s.DuracionMinutos,
                Precio = s.Precio
            });
        }

        public async Task AsignarServicioAsync(int profesionalId, int servicioId)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(profesionalId);
            if (profesional == null)
            {
                throw new NotFoundException("Profesional no encontrado.");
            }

            var servicio = await _servicioRepository.GetByIdAsync(servicioId);
            if (servicio == null)
            {
                throw new NotFoundException("Servicio no encontrado.");
            }

            if (!profesional.Servicios.Any(s => s.Id == servicioId))
            {
                profesional.Servicios.Add(servicio);
                await _profesionalRepository.SaveChangesAsync();
            }
        }

        public async Task RemoverServicioAsync(int profesionalId, int servicioId)
        {
            var profesional = await _profesionalRepository.GetByIdAsync(profesionalId);
            if (profesional == null)
            {
                throw new NotFoundException("Profesional no encontrado.");
            }

            var servicio = profesional.Servicios.FirstOrDefault(s => s.Id == servicioId);
            if (servicio != null)
            {
                profesional.Servicios.Remove(servicio);
                await _profesionalRepository.SaveChangesAsync();
            }
        }
    }
}
