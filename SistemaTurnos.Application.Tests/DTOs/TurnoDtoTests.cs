using SistemaTurnos.Domain.Entities;
using Xunit;

public class TurnoDtoTests
{
    [Fact]
    public void Mapper_CopiaDatosCorrectamente()
    {
        var turno = new Turno(
            1, 1, 1,
            DateTime.Now,
            30
        );

        var dto = turno.ToDto();

        Assert.Equal(turno.PersonaId, dto.PersonaId);
        Assert.Equal(turno.Estado, dto.Estado);
    }
}
