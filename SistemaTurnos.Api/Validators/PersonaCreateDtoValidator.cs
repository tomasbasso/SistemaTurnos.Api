using FluentValidation;
using SistemaTurnos.Application.DTOs;

public class PersonaCreateDtoValidator : AbstractValidator<PersonaCreateDto>
{
    public PersonaCreateDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio")
            .Matches(@"^\d{7,8}$")
            .WithMessage("El DNI debe tener 7 u 8 dígitos");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no es válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("Rol inválido");
    }
}
