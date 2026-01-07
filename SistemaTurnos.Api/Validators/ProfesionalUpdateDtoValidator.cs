using FluentValidation;
using SistemaTurnos.Application.DTOs;

public class ProfesionalUpdateDtoValidator : AbstractValidator<ProfesionalUpdateDto>
{
    public ProfesionalUpdateDtoValidator()
    {
        When(x => x.Matricula != null, () =>
        {
            RuleFor(x => x.Matricula)
                .NotEmpty()
                .MaximumLength(50);
        });
    }
}
