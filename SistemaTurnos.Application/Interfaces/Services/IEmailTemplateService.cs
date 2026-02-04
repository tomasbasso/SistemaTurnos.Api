using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        string GenerateTurnoConfirmacionHtml(Turno turno, Profesional profesional, Servicio servicio);
        string GenerateTurnoCancelacionHtml(Turno turno, string motivo = null);
    }
}
