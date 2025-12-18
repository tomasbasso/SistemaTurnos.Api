using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTurnos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueDniIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Borrar índice incorrecto
            migrationBuilder.DropIndex(
                name: "IX_Personas_Dni",
                table: "Personas");

            // 2. Crear índice único filtrado
            migrationBuilder.Sql(@"
        CREATE UNIQUE INDEX UX_Personas_Dni
        ON Personas (Dni)
        WHERE Activo = 1;
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Borrar índice filtrado
            migrationBuilder.Sql(@"
        DROP INDEX UX_Personas_Dni ON Personas;
    ");

            // 2. Restaurar índice simple
            migrationBuilder.CreateIndex(
                name: "IX_Personas_Dni",
                table: "Personas",
                column: "Dni",
                unique: true);
        }
    }
}
