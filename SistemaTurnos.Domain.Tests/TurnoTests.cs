using System;
using SistemaTurnos.Domain.Entities;
using SistemaTurnos.Domain.Enums;
using Xunit;

namespace SistemaTurnos.Domain.Tests;

public class TurnoTests
{
    [Fact]
    public void Finalizar_TurnoActivo_CambiaEstadoAFinalizado()
    {
        // Arrange
        var turno = CrearTurnoActivo();

        // Act
        turno.Finalizar();

        // Assert
        Assert.Equal(EstadoTurno.Finalizado, turno.Estado);
    }

    private Turno CrearTurnoActivo()
    {
        return new Turno(
            personaId: 1,
            profesionalId: 1,
            servicioId: 1,
            inicio: DateTime.Now.AddMinutes(-30),
            duracionMinutos: 30
        );
    }
    [Fact]
    public void Finalizar_TurnoCancelado_LanzaExcepcion()
    {
        // Arrange
        var turno = CrearTurnoActivo();
        turno.Cancelar();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            turno.Finalizar()
        );
    }

    [Fact]
    public void Finalizar_TurnoYaFinalizado_LanzaExcepcion()
    {
        // Arrange
        var turno = CrearTurnoActivo();
        turno.Finalizar();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            turno.Finalizar()
        );
    }

    [Fact]
    public void Cancelar_TurnoActivo_CambiaEstadoACancelado()
    {
        // Arrange
        var turno = CrearTurnoActivo();

        // Act
        turno.Cancelar();

        // Assert
        Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
    }

    [Fact]
    public void Cancelar_TurnoFinalizado_LanzaExcepcion()
    {
        // Arrange
        var turno = CrearTurnoActivo();
        turno.Finalizar();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            turno.Cancelar()
        );
    }


}
