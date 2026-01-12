using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Domain.Enums;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;

namespace SistemaTurnos.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly ITurnoRepository _turnoRepository;
        // Need to check if I can inject IProfesionalRepository if needed for Name.
        // Or if Turno includes Profesional, I can get name from there.
        // Repository GetTurnosByProfesionalAndDate includes Profesional.

        public ReporteService(ITurnoRepository turnoRepository)
        {
            _turnoRepository = turnoRepository;
        }

        public async Task<ReporteDiarioDto> ObtenerReporteDiarioAsync(DateTime fecha)
        {
            var turnos = await _turnoRepository.GetByFecha(fecha);

            var totalTurnos = turnos.Count();
            var turnosRealizados = turnos.Count(t => t.Estado == EstadoTurno.Finalizado);
            var turnosCancelados = turnos.Count(t => t.Estado == EstadoTurno.Cancelado);

            // Ingresos Estimados (Todos los turnos no cancelados)
            var ingresosEstimados = turnos
                .Where(t => t.Estado != EstadoTurno.Cancelado && t.Servicio != null)
                .Sum(t => t.Servicio.Precio);

            // Ingresos Reales (Solo completados)
            var ingresosReales = turnos
                .Where(t => t.Estado == EstadoTurno.Finalizado && t.Servicio != null)
                .Sum(t => t.Servicio.Precio);

            return new ReporteDiarioDto
            {
                Fecha = fecha,
                CantidadTurnosTotal = totalTurnos,
                CantidadTurnosRealizados = turnosRealizados,
                CantidadTurnosCancelados = turnosCancelados,
                IngresosEstimados = ingresosEstimados,
                IngresosReales = ingresosReales
            };
        }

        public async Task<ReporteMensualDto> ObtenerReporteMensualAsync(int mes, int anio)
        {
            var turnos = await _turnoRepository.GetByMes(mes, anio);

            var totalTurnos = turnos.Count();
            var turnosRealizados = turnos.Count(t => t.Estado == EstadoTurno.Finalizado);
            var turnosCancelados = turnos.Count(t => t.Estado == EstadoTurno.Cancelado);

            var ingresosEstimados = turnos
                .Where(t => t.Estado != EstadoTurno.Cancelado && t.Servicio != null)
                .Sum(t => t.Servicio.Precio);

            var ingresosReales = turnos
                .Where(t => t.Estado == EstadoTurno.Finalizado && t.Servicio != null)
                .Sum(t => t.Servicio.Precio);

            return new ReporteMensualDto
            {
                Mes = mes,
                Anio = anio,
                CantidadTurnosTotal = totalTurnos,
                CantidadTurnosRealizados = turnosRealizados,
                CantidadTurnosCancelados = turnosCancelados,
                IngresosEstimados = ingresosEstimados,
                IngresosReales = ingresosReales
            };
        }

        public async Task<ReporteDiarioProfesionalDto> GenerarReporteDiarioProfesional(int profesionalId, DateTime fecha)
        {
            var turnos = await _turnoRepository.GetTurnosByProfesionalAndDate(profesionalId, fecha);
            
            // Assuming we want return empty list if no turns, but we need Profesional Name.
            // If turns is empty, we can't get name from turns. 
            // In a real app we might inject IProfesionalRepository to get name.
            // For now, I'll use "Desconocido" or try to get from first turno if exists.
            
            string nombreProfesional = "N/A";
            if (turnos.Any())
            {
                var first = turnos.First();
                if (first.Profesional != null && first.Profesional.Persona != null)
                {
                   nombreProfesional = first.Profesional.Persona.Nombre; 
                }
            }

            var totalTurnos = turnos.Count();
            var confirmados = turnos.Count(t => t.Estado == EstadoTurno.Activo);
            var cancelados = turnos.Count(t => t.Estado == EstadoTurno.Cancelado);
            // Pendientes?? Assuming Activo is the only "Pending" state.
             var pendientes = 0; 

            return new ReporteDiarioProfesionalDto
            {
                ProfesionalId = profesionalId,
                NombreProfesional = nombreProfesional,
                Fecha = fecha,
                TotalTurnos = totalTurnos,
                TurnosConfirmados = confirmados,
                TurnosPendientes = pendientes,
                TurnosCancelados = cancelados,
                Turnos = turnos.Select(MapTurnoToDto).ToList()
            };
        }

        private TurnoDto MapTurnoToDto(Turno t)
        {
             return new TurnoDto
             {
                 Id = t.Id,
                 PersonaId = t.PersonaId,
                 ProfesionalId = t.ProfesionalId,
                 ServicioId = t.ServicioId,
                 FechaHoraInicio = t.FechaHoraInicio,
                 FechaHoraFin = t.FechaHoraFin,
                 Estado = t.Estado
             };
        }
    }
}
