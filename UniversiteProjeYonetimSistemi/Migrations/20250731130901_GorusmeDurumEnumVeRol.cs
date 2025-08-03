using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversiteProjeYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class GorusmeDurumEnumVeRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add a new temporary integer column
            migrationBuilder.AddColumn<int>(
                name: "DurumInt",
                table: "DanismanlikGorusmeleri",
                nullable: true);

            // 2. Update the new column with data from the old string column
            migrationBuilder.Sql("UPDATE DanismanlikGorusmeleri SET DurumInt = CASE Durum " +
                               "WHEN 'HocaOnayiBekliyor' THEN 0 " +
                               "WHEN 'OgrenciOnayiBekliyor' THEN 1 " +
                               "WHEN 'Onaylandi' THEN 2 " +
                               "WHEN 'IptalEdildi' THEN 3 " +
                               "WHEN 'Tamamlandi' THEN 4 " +
                               "WHEN 'Beklemede' THEN 0 " +
                               "ELSE 0 END");

            // 3. Drop the old string column
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "DanismanlikGorusmeleri");

            // 4. Rename the new column to 'Durum'
            migrationBuilder.RenameColumn(
                name: "DurumInt",
                table: "DanismanlikGorusmeleri",
                newName: "Durum");

            // 5. Make the new 'Durum' column non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "Durum",
                table: "DanismanlikGorusmeleri",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SonGuncelleyenRol",
                table: "DanismanlikGorusmeleri",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SonGuncelleyenRol",
                table: "DanismanlikGorusmeleri");

            migrationBuilder.AlterColumn<string>(
                name: "Durum",
                table: "DanismanlikGorusmeleri",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
