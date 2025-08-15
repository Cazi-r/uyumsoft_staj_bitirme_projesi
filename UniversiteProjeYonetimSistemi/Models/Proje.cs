using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class Proje : TemelVarlik
    {
        [Required]
        [StringLength(100)]
        public string Ad { get; set; }
        
        [Required]
        public string Aciklama { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime? TeslimTarihi { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Beklemede"; // Beklemede, Atanmis, Devam, Tamamlandi, Iptal
        
        public int? OgrenciId { get; set; }
        public Ogrenci Ogrenci { get; set; }
        
        public int? MentorId { get; set; }
        
        [ForeignKey("MentorId")]
        public Akademisyen Mentor { get; set; }
        
        public int? KategoriId { get; set; }
        
        [ForeignKey("KategoriId")]
        public ProjeKategori Kategori { get; set; }
        
        // Navigation properties
        public ICollection<ProjeDosya> Dosyalar { get; set; }
        public ICollection<ProjeYorum> Yorumlar { get; set; }
        public ICollection<Degerlendirme> Degerlendirmeler { get; set; }
        public ICollection<ProjeAsamasi> Asamalar { get; set; }
        public ICollection<ProjeKaynagi> Kaynaklar { get; set; }
    }
} 