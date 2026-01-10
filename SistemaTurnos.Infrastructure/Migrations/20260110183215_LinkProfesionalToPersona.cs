using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTurnos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkProfesionalToPersona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Profesionales");

            migrationBuilder.AddColumn<int>(
                name: "PersonaId",
                table: "Profesionales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Profesionales_PersonaId",
                table: "Profesionales",
                column: "PersonaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Profesionales_Personas_PersonaId",
                table: "Profesionales",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profesionales_Personas_PersonaId",
                table: "Profesionales");

            migrationBuilder.DropIndex(
                name: "IX_Profesionales_PersonaId",
                table: "Profesionales");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "Profesionales");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Profesionales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
