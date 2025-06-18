using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionApp.Migrations
{
    /// <inheritdoc />
    public partial class FixNullSafetyInFacturax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Empresas",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Empresas");
        }
    }
}
