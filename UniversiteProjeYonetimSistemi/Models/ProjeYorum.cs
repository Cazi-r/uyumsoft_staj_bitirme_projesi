using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class ProjeYorum : TemelVarlik
    {
        [Required]
        public string Icerik { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        [StringLength(20)]
        public string YorumTipi { get; set; } = "Genel"; // Genel, GeriBildirim, Soru, Onay
        
        [ForeignKey("Proje")]
        public int ProjeId { get; set; }
        public Proje Proje { get; set; }
        
        public int? OgrenciId { get; set; }
        public Ogrenci Ogrenci { get; set; }
        
        public int? AkademisyenId { get; set; }
        public Akademisyen Akademisyen { get; set; }
    }
} 