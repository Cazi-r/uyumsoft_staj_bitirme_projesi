using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class Bildirim : TemelVarlik
    {
        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }
        
        [Required]
        public string Icerik { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public bool Okundu { get; set; } = false;
        
        [StringLength(20)]
        public string BildirimTipi { get; set; } = "Bilgi"; // Bilgi, Uyari, Hata, Basari
        
        public int? OgrenciId { get; set; }
        public Ogrenci Ogrenci { get; set; }
        
        public int? AkademisyenId { get; set; }
        public Akademisyen Akademisyen { get; set; }
    }
} 