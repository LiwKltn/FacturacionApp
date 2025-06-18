using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionApp.Migrations
{
    /// <inheritdoc />
    public partial class FixFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Empresas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
