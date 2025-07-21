using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<ProjeKategori> ProjeKategorileri { get; set; }
        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<Akademisyen> Akademisyenler { get; set; }
        public DbSet<Proje> Projeler { get; set; }
        public DbSet<ProjeDosya> ProjeDosyalari { get; set; }
        public DbSet<ProjeYorum> ProjeYorumlari { get; set; }
        public DbSet<Degerlendirme> Degerlendirmeler { get; set; }
        public DbSet<Bildirim> Bildirimler { get; set; }
        public DbSet<ProjeAsamasi> ProjeAsamalari { get; set; }
        public DbSet<ProjeKaynagi> ProjeKaynaklari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Kullanıcı rol kısıtlaması
            modelBuilder.Entity<Kullanici>()
                .Property(k => k.Rol)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Proje - Öğrenci ilişkisi
            modelBuilder.Entity<Proje>()
                .HasOne(p => p.Ogrenci)
                .WithMany(o => o.Projeler)
                .HasForeignKey(p => p.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Proje - Mentor (Akademisyen) ilişkisi
            modelBuilder.Entity<Proje>()
                .HasOne(p => p.Mentor)
                .WithMany(a => a.Projeler)
                .HasForeignKey(p => p.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Proje - Kategori ilişkisi
            modelBuilder.Entity<Proje>()
                .HasOne(p => p.Kategori)
                .WithMany(k => k.Projeler)
                .HasForeignKey(p => p.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjeDosya - Proje ilişkisi
            modelBuilder.Entity<ProjeDosya>()
                .HasOne(d => d.Proje)
                .WithMany(p => p.Dosyalar)
                .HasForeignKey(d => d.ProjeId);

            // ProjeYorum kontrol kısıtlaması (Öğrenci veya Akademisyen)
            modelBuilder.Entity<ProjeYorum>()
                .ToTable(tb => tb.HasCheckConstraint("CHK_YorumYapan",
                    "(OgrenciId IS NOT NULL AND AkademisyenId IS NULL) OR (OgrenciId IS NULL AND AkademisyenId IS NOT NULL)"));

            // Bildirim kontrol kısıtlaması (Öğrenci veya Akademisyen)
            modelBuilder.Entity<Bildirim>()
                .ToTable(tb => tb.HasCheckConstraint("CHK_BildirimAlici",
                    "(OgrenciId IS NOT NULL AND AkademisyenId IS NULL) OR (OgrenciId IS NULL AND AkademisyenId IS NOT NULL)"));
        }
    }
}
