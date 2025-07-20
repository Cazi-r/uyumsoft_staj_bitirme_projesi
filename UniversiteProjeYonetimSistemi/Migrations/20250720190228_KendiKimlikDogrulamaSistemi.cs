using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversiteProjeYonetimSistemi.Migrations
{
    /// <inheritdoc />
    public partial class KendiKimlikDogrulamaSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SonGiris = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjeKategorileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Renk = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeKategorileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Akademisyenler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Ofis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UzmanlikAlani = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Akademisyenler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Akademisyenler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OgrenciNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenciler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Okundu = table.Column<bool>(type: "bit", nullable: false),
                    BildirimTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OgrenciId = table.Column<int>(type: "int", nullable: true),
                    AkademisyenId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                    table.CheckConstraint("CHK_BildirimAlici", "(OgrenciId IS NOT NULL AND AkademisyenId IS NULL) OR (OgrenciId IS NULL AND AkademisyenId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Bildirimler_Akademisyenler_AkademisyenId",
                        column: x => x.AkademisyenId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bildirimler_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeslimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OgrenciId = table.Column<int>(type: "int", nullable: true),
                    MentorId = table.Column<int>(type: "int", nullable: true),
                    KategoriId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projeler_Akademisyenler_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projeler_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projeler_ProjeKategorileri_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "ProjeKategorileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Degerlendirmeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DegerlendirmeTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjeId = table.Column<int>(type: "int", nullable: false),
                    AkademisyenId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degerlendirmeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Akademisyenler_AkademisyenId",
                        column: x => x.AkademisyenId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeAsamalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjeId = table.Column<int>(type: "int", nullable: false),
                    AsamaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SiraNo = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeAsamalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeAsamalari_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeDosyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DosyaAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosyaTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DosyaBoyutu = table.Column<long>(type: "bigint", nullable: false),
                    YuklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjeId = table.Column<int>(type: "int", nullable: false),
                    YukleyenId = table.Column<int>(type: "int", nullable: true),
                    YukleyenTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeDosyalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeDosyalari_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeKaynaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjeId = table.Column<int>(type: "int", nullable: false),
                    KaynakAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KaynakTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Yazar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeKaynaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeKaynaklari_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeYorumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YorumTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjeId = table.Column<int>(type: "int", nullable: false),
                    OgrenciId = table.Column<int>(type: "int", nullable: true),
                    AkademisyenId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeYorumlari", x => x.Id);
                    table.CheckConstraint("CHK_YorumYapan", "(OgrenciId IS NOT NULL AND AkademisyenId IS NULL) OR (OgrenciId IS NULL AND AkademisyenId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ProjeYorumlari_Akademisyenler_AkademisyenId",
                        column: x => x.AkademisyenId,
                        principalTable: "Akademisyenler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjeYorumlari_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjeYorumlari_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Akademisyenler_KullaniciId",
                table: "Akademisyenler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_AkademisyenId",
                table: "Bildirimler",
                column: "AkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_OgrenciId",
                table: "Bildirimler",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_AkademisyenId",
                table: "Degerlendirmeler",
                column: "AkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_ProjeId",
                table: "Degerlendirmeler",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_KullaniciId",
                table: "Ogrenciler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeAsamalari_ProjeId",
                table: "ProjeAsamalari",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeDosyalari_ProjeId",
                table: "ProjeDosyalari",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeKaynaklari_ProjeId",
                table: "ProjeKaynaklari",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_KategoriId",
                table: "Projeler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_MentorId",
                table: "Projeler",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_OgrenciId",
                table: "Projeler",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeYorumlari_AkademisyenId",
                table: "ProjeYorumlari",
                column: "AkademisyenId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeYorumlari_OgrenciId",
                table: "ProjeYorumlari",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeYorumlari_ProjeId",
                table: "ProjeYorumlari",
                column: "ProjeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "Degerlendirmeler");

            migrationBuilder.DropTable(
                name: "ProjeAsamalari");

            migrationBuilder.DropTable(
                name: "ProjeDosyalari");

            migrationBuilder.DropTable(
                name: "ProjeKaynaklari");

            migrationBuilder.DropTable(
                name: "ProjeYorumlari");

            migrationBuilder.DropTable(
                name: "Projeler");

            migrationBuilder.DropTable(
                name: "Akademisyenler");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "ProjeKategorileri");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
