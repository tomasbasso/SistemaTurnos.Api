using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTurnos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTurnoProfesionalRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Profesionales_ProfesionalId1",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_ProfesionalId1",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "ProfesionalId1",
                table: "Turnos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfesionalId1",
                table: "Turnos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_ProfesionalId1",
                table: "Turnos",
                column: "ProfesionalId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Profesionales_ProfesionalId1",
                table: "Turnos",
                column: "ProfesionalId1",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
