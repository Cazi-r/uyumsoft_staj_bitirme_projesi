using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class ProjeKaynagi : TemelVarlik
    {
        [ForeignKey("Proje")]
        public int ProjeId { get; set; }
        public Proje Proje { get; set; }
        
        [Required]
        [StringLength(200)]
        public string KaynakAdi { get; set; }
        
        [StringLength(50)]
        public string KaynakTipi { get; set; } // Kitap, Makale, Website, API, Dokuman, Video, Diger
        
        public string Url { get; set; }
        
        public string Aciklama { get; set; }
        
        [StringLength(100)]
        public string Yazar { get; set; }
        
        public DateTime? YayinTarihi { get; set; }
        
        public DateTime EklemeTarihi { get; set; } = DateTime.Now;
    }
} 