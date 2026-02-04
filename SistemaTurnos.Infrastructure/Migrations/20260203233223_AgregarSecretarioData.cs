using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTurnos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSecretarioData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoConsulta",
                table: "Turnos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObraSocial",
                table: "Personas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoConsulta",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "ObraSocial",
                table: "Personas");
        }
    }
}
