using FluentValidation;
using SistemaTurnos.Application.DTOs;

public class ServicioCreateDtoValidator : AbstractValidator<ServicioCreateDto>
{
    public ServicioCreateDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Descripcion)
            .MaximumLength(500);

        RuleFor(x => x.DuracionMinutos)
            .GreaterThan(0);

        RuleFor(x => x.Precio)
            .GreaterThan(0);
    }
}