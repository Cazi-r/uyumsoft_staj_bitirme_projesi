using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversiteProjeYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddZamanDurumuToDanismanlikGorusmesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZamanDurumu",
                table: "DanismanlikGorusmeleri",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZamanDurumu",
                table: "DanismanlikGorusmeleri");
        }
    }
}
