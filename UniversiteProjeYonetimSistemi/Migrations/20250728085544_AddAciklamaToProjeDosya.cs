using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversiteProjeYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddAciklamaToProjeDosya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "ProjeDosyalari",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "ProjeDosyalari");
        }
    }
}
