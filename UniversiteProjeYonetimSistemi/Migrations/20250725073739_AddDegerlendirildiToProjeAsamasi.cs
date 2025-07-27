using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversiteProjeYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddDegerlendirildiToProjeAsamasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Degerlendirildi",
                table: "ProjeAsamalari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DegerlendirmeTarihi",
                table: "ProjeAsamalari",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Degerlendirildi",
                table: "ProjeAsamalari");

            migrationBuilder.DropColumn(
                name: "DegerlendirmeTarihi",
                table: "ProjeAsamalari");
        }
    }
}
