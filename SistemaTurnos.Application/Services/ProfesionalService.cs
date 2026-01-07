using SistemaTurnos.Application.DTOs;
using SistemaTurnos.Application.DTOs.Common;
using SistemaTurnos.Application.Interfaces;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Exceptions;

public class ProfesionalService : IProfesionalService
{
    private readonly IProfesionalRepository _repository;

    public ProfesionalService(IProfesionalRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProfesionalDto> CrearAsync(ProfesionalCreateDto dto)
    {
        if (await _repository.ExisteMatriculaAsync(dto.Matricula))
            throw new BusinessException("La matrícula ya existe");

        var profesional = new Profesional(dto.Nombre, dto.Matricula);

        await _repository.AddAsync(profesional);

        return MapToDto(profesional);
    }

    public async Task ActualizarAsync(int id, ProfesionalUpdateDto dto)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException("Profesional no encontrado");

        if (dto.Matricula != null &&
            dto.Matricula != profesional.Matricula &&
            await _repository.ExisteMatriculaAsync(dto.Matricula, id))
            throw new BusinessException("La matrícula ya está en uso");

        if (dto.Nombre != null)
            profesional.Nombre = dto.Nombre;

        if (dto.Matricula != null)
            profesional.Matricula = dto.Matricula;

        await _repository.SaveChangesAsync();
    }


    public async Task EliminarAsync(int id)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException("Profesional no encontrado");

        profesional.Activo = false;
        await _repository.SaveChangesAsync();
    }

    public async Task ReactivarAsync(int id)
    {
        var profesional = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException("Profesional no encontrado");

        profesional.Activo = true;
        await _repository.SaveChangesAsync();
    }

    public async Task<ProfesionalDto?> GetByIdAsync(int id)
    {
        var profesional = await _repository.GetByIdAsync(id);

        return profesional == null || !profesional.Activo
            ? null
            : MapToDto(profesional);
    }

    public async Task<PagedResultDto<ProfesionalDto>> GetPagedAsync(
        string? busqueda,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        var (items, total) = await _repository.GetPagedAsync(
            busqueda, page, pageSize, sortBy, sortDir);

        return new PagedResultDto<ProfesionalDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items.Select(MapToDto).ToList()
        };
    }

    private static ProfesionalDto MapToDto(Profesional p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Matricula = p.Matricula
    };
}
