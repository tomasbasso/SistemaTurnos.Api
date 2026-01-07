using SistemaTurnos.Domain.Entities;
using System.Linq.Expressions;

namespace SistemaTurnos.Application.Common
{
    public static class PersonaSortFields
    {
        public static readonly Dictionary<string, Expression<Func<Persona, object>>> Map =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["id"] = p => p.Id,
                ["nombre"] = p => p.Nombre,
                ["dni"] = p => p.Dni,
                ["email"] = p => p.Email
            };
    }
}
