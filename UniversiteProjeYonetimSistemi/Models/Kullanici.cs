using System;
using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class Kullanici : TemelVarlik
    {
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
        [StringLength(255)]
        public string Sifre { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Rol { get; set; } // Admin, Akademisyen, Ogrenci
        
        public bool Aktif { get; set; } = true;
        public DateTime? SonGiris { get; set; }
    }
} 