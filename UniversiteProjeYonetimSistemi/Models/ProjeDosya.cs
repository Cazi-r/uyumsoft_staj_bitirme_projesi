using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class ProjeDosya : TemelVarlik
    {
        [Required]
        [StringLength(255)]
        public string DosyaAdi { get; set; }
        
        [Required]
        public string DosyaYolu { get; set; }
        
        [StringLength(50)]
        public string DosyaTipi { get; set; }
        
        public long DosyaBoyutu { get; set; } = 0;
        
        public DateTime YuklemeTarihi { get; set; } = DateTime.Now;
        
        [ForeignKey("Proje")]
        public int ProjeId { get; set; }
        public Proje Proje { get; set; }
        
        public int? YukleyenId { get; set; }
        
        [StringLength(20)]
        public string YukleyenTipi { get; set; } = "Ogrenci"; // Ogrenci, Akademisyen
    }
} 