using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;

namespace SistemaTurnos.Application.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;

        public ServicioService(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServicioDto> CrearAsync(ServicioCreateDto dto)
        {
            var servicio = new Servicio(dto.Nombre, dto.Descripcion, dto.DuracionMinutos, dto.Precio);

            await _repository.AddAsync(servicio);

            return Map(servicio);
        }

        public async Task ActualizarAsync(int id, ServicioUpdateDto dto)
        {
            var servicio = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Servicio no encontrado");

            servicio.Actualizar(
                dto.Nombre ?? servicio.Nombre,
                dto.Descripcion ?? servicio.Descripcion,
                dto.DuracionMinutos ?? servicio.DuracionMinutos,
                dto.Precio ?? servicio.Precio);

            await _repository.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var servicio = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Servicio no encontrado");

            if (!servicio.Activo)
                throw new BusinessException("El servicio ya está desactivado");

            servicio.Activo = false;
            await _repository.SaveChangesAsync();
        }

        public async Task ReactivarAsync(int id)
        {
            var servicio = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Servicio no encontrado");

            if (servicio.Activo)
                throw new BusinessException("El servicio ya está activo");

            servicio.Activo = true;
            await _repository.SaveChangesAsync();
        }


        public async Task<ServicioDto?> GetByIdAsync(int id)
        {
            var servicio = await _repository.GetByIdAsync(id);

            return servicio == null || !servicio.Activo
                ? null
                : Map(servicio);
        }

        public async Task<PagedResultDto<ServicioDto>> GetPagedAsync(
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy,
            string? sortDir)
        {
            var (items, total) = await _repository.GetPagedAsync(
                busqueda, page, pageSize, sortBy, sortDir);

            return new PagedResultDto<ServicioDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items.Select(Map).ToList()
            };
        }

        private static ServicioDto Map(Servicio s) => new()
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Descripcion = s.Descripcion,
            DuracionMinutos = s.DuracionMinutos,
            Precio = s.Precio
        };
    }
}
