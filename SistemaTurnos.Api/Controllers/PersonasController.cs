using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Api.DTOs;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly SistemaTurnosDbContext _context;

    public PersonasController(SistemaTurnosDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post(PersonaCreateDto dto)
    {
        var errorValidacion = ValidarPersona(dto);
            if (errorValidacion != null)
            {
                return errorValidacion;
            }

        // 4. Crear y guardar
        var persona = new Persona
        {
            Nombre = dto.Nombre,
            Dni = dto.Dni,
            Email = dto.Email,
            Activo = true
        };

        _context.Personas.Add(persona);
        try
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_Personas_Dni") == true)
        {
            return Conflict($"Ya existe una persona activa con el DNI {dto.Dni}");
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = persona.Id },
            new PersonaDto
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Dni = persona.Dni,
                Email = persona.Email
            });
    }

    //VALIDACIONES DE PERSONA
    private IActionResult? ValidarPersona(PersonaCreateDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Nombre) ||
        string.IsNullOrWhiteSpace(dto.Dni) ||
        string.IsNullOrWhiteSpace(dto.Email))
    {
        return BadRequest("Nombre, DNI y Email son obligatorios.");
    }

    string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    if (!Regex.IsMatch(dto.Email, emailPattern))
    {
        return BadRequest("El formato del correo electrónico no es válido.");
    }

    string dniPattern = @"^\d{7,8}$";
    if (!Regex.IsMatch(dto.Dni, dniPattern))
    {
        return BadRequest("El DNI debe contener solo números y tener entre 7 y 8 dígitos.");
    }

    return null; // todo OK
}

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, PersonaCreateDto dto)
    {
            var errorValidacion = ValidarPersona(dto);
        if (errorValidacion != null)
        {
            return errorValidacion;
        }
        // 2. Verificar si existe la persona que queremos editar
        var persona = await _context.Personas.FindAsync(id);

        if (persona == null)
        {
            return NotFound($"No se encontró una persona con el ID {id}");
        }

   
        // 4. Actualizar los datos de la entidad
        persona.Nombre = dto.Nombre;
        persona.Dni = dto.Dni;
        persona.Email = dto.Email;
        // Nota: Generalmente no actualizamos el Id ni el campo Activo aquí, a menos que sea la intención.

        // 5. Guardar cambios
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Doble check de seguridad por si fue borrado concurrentemente
            if (!await _context.Personas.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Personas_Dni"))
            {
                return Conflict($"El DNI {dto.Dni} ya está siendo utilizado por otra persona.");
            }
            throw;
        }

        // 6. Retornar respuesta
        // NoContent (204) es el estándar para PUT cuando no devuelves datos nuevos, 
        // pero indica que la operación fue exitosa.
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // 1. Buscamos la persona
        var persona = await _context.Personas.FindAsync(id);

        // 2. Verificamos si existe
        if (persona == null)
        {
            return NotFound($"No se encontró la persona con ID {id}");
        }

        // 3. CAMBIO DE ESTADO (Borrado lógico)
        // En lugar de borrar, lo desactivamos.
        persona.Activo = false;

        // 4. Guardamos los cambios
        await _context.SaveChangesAsync();

        return NoContent(); // Retorna 204 (Éxito sin contenido)
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var persona = await _context.Personas
            .Where(p => p.Activo == true) // Solo activos
            .FirstOrDefaultAsync(p => p.Id == id);

        if (persona == null)
        {
            return NotFound($"No se encontró la persona con ID {id}");
        }

        var dto = new PersonaDto
        {
            Id = persona.Id,
            Nombre = persona.Nombre,
            Dni = persona.Dni,
            Email = persona.Email
        };

        return Ok(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busqueda)
    {
        // 1. Empezamos la consulta base (solo activos para mantener el borrado lógico)
        var query = _context.Personas.Where(p => p.Activo == true).AsQueryable();

        // 2. Si el usuario escribió algo, filtramos
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p => p.Nombre.Contains(busqueda) || p.Dni.Contains(busqueda));
        }

        // 3. Ejecutamos la consulta y transformamos a DTO
        var personas = await query
            .Select(p => new PersonaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Dni = p.Dni,
                Email = p.Email
            })
            .ToListAsync();

        return Ok(personas);
    }
    //REACTIVAR BORRADO 
    [HttpPut("Reactivar/{id}")]
    public async Task<IActionResult> Reactivar(int id)
    {
        // Buscamos incluso si Activo es false (ignoramos el filtro global si tuvieras uno)
        var persona = await _context.Personas.FindAsync(id);

        if (persona == null)
        {
            return NotFound($"No existe el registro con ID {id}");
        }

        if (persona.Activo)
        {
            return BadRequest("La persona ya está activa.");
        }

        persona.Activo = true;
        await _context.SaveChangesAsync();

        return NoContent(); // O Ok("Persona reactivada correctamente");
    }
}
