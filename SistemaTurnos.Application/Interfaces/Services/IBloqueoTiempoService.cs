using SistemaTurnos.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IBloqueoTiempoService
    {
        Task<IEnumerable<BloqueoTiempoDto>> GetByProfesionalIdAsync(int profesionalId, DateTime desde, DateTime hasta);
        Task<BloqueoTiempoDto> CreateAsync(int profesionalId, BloqueoTiempoCreateDto createDto);
        Task DeleteAsync(int id);
    }
}
