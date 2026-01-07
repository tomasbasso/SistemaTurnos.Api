using FluentValidation;
using SistemaTurnos.Application.DTOs;

public class ServicioUpdateDtoValidator : AbstractValidator<ServicioUpdateDto>
{
    public ServicioUpdateDtoValidator()
    {
        When(x => x.Nombre != null, () =>
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.Descripcion != null, () =>
        {
            RuleFor(x => x.Descripcion)
                .MaximumLength(500);
        });

        When(x => x.DuracionMinutos.HasValue, () =>
        {
            RuleFor(x => x.DuracionMinutos.Value)
                .GreaterThan(0);
        });

        When(x => x.Precio.HasValue, () =>
        {
            RuleFor(x => x.Precio.Value)
                .GreaterThan(0);
        });
    }
}