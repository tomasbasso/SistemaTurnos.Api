using System.Threading.Tasks;

namespace SistemaTurnos.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(int? usuarioId, string accion, string entidad, string detalle, string? ipAddress = null);
    }
}
