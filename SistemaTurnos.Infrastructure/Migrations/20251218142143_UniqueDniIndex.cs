using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTurnos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueDniIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Personas_Dni",
                table: "Personas",
                column: "Dni",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Personas_Dni",
                table: "Personas");
        }
    }
}
