using FluentValidation;
using SistemaTurnos.Application.DTOs;

public class PersonaUpdateDtoValidator : AbstractValidator<PersonaUpdateDto>
{
    public PersonaUpdateDtoValidator()
    {
        When(x => x.Nombre != null, () =>
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.Dni != null, () =>
        {
            RuleFor(x => x.Dni)
                .Matches(@"^\d{7,8}$")
                .WithMessage("El DNI debe tener 7 u 8 dígitos");
        });

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("El email no es válido");
        });

        When(x => x.Rol.HasValue, () =>
        {
            RuleFor(x => x.Rol.Value)
                .IsInEnum()
                .WithMessage("Rol inválido");
        });
    }
}
