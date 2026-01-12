using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
﻿[Route("api/turnos")]
﻿[Authorize]
﻿public class TurnosController : ControllerBase
﻿{
﻿    private readonly TurnoService _service;
﻿
﻿    public TurnosController(TurnoService service)
﻿    {
﻿        _service = service;
﻿    }
﻿
﻿    // --------------------
﻿    // CREAR TURNO
﻿    // --------------------
﻿    [HttpPost]
﻿    [Authorize(Roles = "Cliente, Administrador, Profesional")]
﻿    public async Task<IActionResult> Crear([FromBody] TurnoCreateDto dto)
﻿    {
﻿        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
﻿        var turno = await _service.CrearAsync(dto, userId);
﻿        return CreatedAtAction(
﻿            nameof(GetById),
﻿            new { id = turno.Id },
﻿            turno.ToDto()
﻿        );
﻿    }﻿
﻿    // --------------------
﻿    // OBTENER POR ID
﻿    // --------------------
﻿    [HttpGet("{id}")]
﻿    [Authorize(Roles = "Administrador, Profesional, Cliente")]
﻿    public async Task<IActionResult> GetById(int id)
﻿    {
﻿        var turno = await _service.GetByIdAsync(id);
﻿
﻿        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
﻿        var userRole = User.FindFirstValue(ClaimTypes.Role);
﻿
﻿        if (userRole != "Administrador" && userRole != "Profesional" && turno.PersonaId.ToString() != userId)
﻿        {
﻿            return Forbid();
﻿        }
﻿
﻿        return Ok(turno.ToDto());
﻿    }
﻿
﻿    // --------------------
﻿    // CANCELAR
﻿    // --------------------
﻿    [HttpPut("{id}/cancelar")]
﻿    [Authorize(Roles = "Administrador, Profesional, Cliente")]
﻿    public async Task<IActionResult> Cancelar(int id)
﻿    {
﻿        var turno = await _service.GetByIdAsync(id);
﻿
﻿        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
﻿        var userRole = User.FindFirstValue(ClaimTypes.Role);
﻿
﻿        if (userRole != "Administrador" && userRole != "Profesional" && turno.PersonaId.ToString() != userId)
﻿        {
﻿            return Forbid();
﻿        }
﻿
﻿        await _service.CancelarAsync(id);
﻿        return NoContent();
﻿    }﻿
﻿    // --------------------
﻿    // FINALIZAR
﻿    // --------------------
﻿    [HttpPut("{id}/finalizar")]
﻿    [Authorize(Roles = "Administrador, Profesional")]
﻿    public async Task<IActionResult> Finalizar(int id)
﻿    {
﻿        await _service.FinalizarAsync(id);
﻿        return NoContent();
﻿    }
﻿
﻿    // --------------------
﻿    // AGENDA PROFESIONAL
﻿    // --------------------
﻿    [HttpGet("profesionales/{profesionalId}/agenda")]
﻿    [Authorize(Roles = "Profesional,Administrador")]
﻿    public async Task<IActionResult> AgendaProfesional(
﻿    int profesionalId,
﻿    DateTime? desde,
﻿    DateTime? hasta)
﻿    {
﻿        var agenda = await _service.ObtenerAgendaProfesionalAsync(
﻿            profesionalId,
﻿            desde,
﻿            hasta
﻿        );
﻿
﻿        return Ok(agenda);
﻿    }
﻿
﻿        // --------------------
﻿        // MIS TURNOS (CLIENTE)
﻿        // --------------------
﻿        [HttpGet("mis-turnos")]
﻿        [Authorize(Roles = "Cliente")]
﻿        public async Task<IActionResult> MisTurnos()
﻿        {
﻿            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
﻿            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
﻿            {
﻿                return Unauthorized("ID de usuario no encontrado en el token.");
﻿            }
﻿            var turnos = await _service.GetTurnosByPersonaIdAsync(userId);
﻿            return Ok(turnos.Select(t => t.ToDto()));
﻿        }﻿
﻿    // --------------------
﻿    // TODOS LOS TURNOS (ADMIN Y PROFESIONAL)
﻿    // --------------------
﻿    [HttpGet("todos")]
﻿    [Authorize(Roles = "Administrador, Profesional")]
﻿    public async Task<IActionResult> TodosLosTurnos()
﻿    {
﻿        var turnos = await _service.GetAllTurnosAsync();
﻿        return Ok(turnos.Select(t => t.ToDto()));
﻿    }
﻿}
