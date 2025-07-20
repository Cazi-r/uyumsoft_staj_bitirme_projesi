using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class Degerlendirme : TemelVarlik
    {
        [Range(0, 100)]
        public int Puan { get; set; }
        
        public string Aciklama { get; set; }
        
        public DateTime DegerlendirmeTarihi { get; set; } = DateTime.Now;
        
        [StringLength(20)]
        public string DegerlendirmeTipi { get; set; } = "Genel"; // Ara, Final, Genel
        
        [ForeignKey("Proje")]
        public int ProjeId { get; set; }
        public Proje Proje { get; set; }
        
        [ForeignKey("Akademisyen")]
        public int AkademisyenId { get; set; }
        public Akademisyen Akademisyen { get; set; }
    }
} 