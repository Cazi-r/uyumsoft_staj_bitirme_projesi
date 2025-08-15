using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class Ogrenci : TemelVarlik
    {
        [ForeignKey("Kullanici")]
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Ad { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Soyad { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(10)]
        public string OgrenciNo { get; set; }
        
        [StringLength(15)]
        public string Telefon { get; set; }
        
        public string Adres { get; set; }
        
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<Proje> Projeler { get; set; }
        public ICollection<ProjeYorum> Yorumlar { get; set; }
        public ICollection<Bildirim> Bildirimler { get; set; }
    }
} 