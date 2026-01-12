using SistemaTurnos.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IReporteService
    {
        Task<ReporteDiarioProfesionalDto> GenerarReporteDiarioProfesional(int profesionalId, DateTime fecha);
        Task<ReporteDiarioDto> ObtenerReporteDiarioAsync(DateTime fecha);
        Task<ReporteMensualDto> ObtenerReporteMensualAsync(int mes, int anio);
    }
}
