using Microsoft.AspNetCore.Hosting;
using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.Interfaces.Repositories;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaTurnos.Application.Services
{
    public class HistorialClinicoService : IHistorialClinicoService
    {
        private readonly INotaClinicaRepository _notaRepository;
        private readonly ITurnoRepository _turnoRepository; // We need this to get PersonaId
        private readonly IWebHostEnvironment _environment;

        public HistorialClinicoService(
            INotaClinicaRepository notaRepository,
            ITurnoRepository turnoRepository,
            IWebHostEnvironment environment)
        {
            _notaRepository = notaRepository;
            _turnoRepository = turnoRepository;
            _environment = environment;
        }

        public async Task<NotaClinicaDto> CrearNotaAsync(NotaClinicaCreateDto dto)
        {
            try
            {
                var turno = await _turnoRepository.GetByIdAsync(dto.TurnoId);
                if (turno == null) throw new Exception("Turno no encontrado");

                var nota = new NotaClinica
                {
                    TurnoId = dto.TurnoId,
                    Contenido = dto.Contenido,
                    VisibleParaPaciente = dto.VisibleParaPaciente,
                    FechaCreacion = DateTime.Now
                };

                // Manejo de archivos
                if (dto.Archivos != null && dto.Archivos.Count > 0)
                {
                    var webRoot = _environment.WebRootPath;
                    if (string.IsNullOrEmpty(webRoot))
                    {
                        webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    }

                    var uploadsFolder = Path.Combine(webRoot, "uploads", "historias_clinicas", turno.PersonaId.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in dto.Archivos)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            nota.ArchivosAdjuntos.Add(new ArchivoAdjunto
                            {
                                NombreOriginal = file.FileName,
                                TipoArchivo = file.ContentType,
                                TamanioBytes = file.Length,
                                RutaArchivo = Path.Combine("uploads", "historias_clinicas", turno.PersonaId.ToString(), fileName).Replace("\\", "/")
                            });
                        }
                    }
                }

                await _notaRepository.AddAsync(nota);

                return MapToDto(nota);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] Error creating note: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                if(ex.InnerException != null)
                {
                    Console.WriteLine($"[INNER ERROR] {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<IEnumerable<NotaClinicaDto>> GetByTurnoIdAsync(int turnoId)
        {
            var notas = await _notaRepository.GetByTurnoIdAsync(turnoId);
            return notas.Select(MapToDto);
        }

        public async Task<IEnumerable<NotaClinicaDto>> GetByPersonaIdAsync(int personaId)
        {
             var notas = await _notaRepository.GetByPersonaIdAsync(personaId);
            return notas.Select(MapToDto);
        }

        private NotaClinicaDto MapToDto(NotaClinica nota)
        {
            return new NotaClinicaDto
            {
                Id = nota.Id,
                TurnoId = nota.TurnoId,
                Contenido = nota.Contenido,
                FechaCreacion = nota.FechaCreacion,
                VisibleParaPaciente = nota.VisibleParaPaciente,
                Archivos = nota.ArchivosAdjuntos.Select(a => new ArchivoAdjuntoDto
                {
                    Id = a.Id,
                    NombreOriginal = a.NombreOriginal,
                    TipoArchivo = a.TipoArchivo,
                    Url = "/" + a.RutaArchivo 
                }).ToList()
            };
        }
    }
}

