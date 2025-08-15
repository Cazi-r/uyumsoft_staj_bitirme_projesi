using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class ProjeKategori : TemelVarlik
    {
        [Required]
        [StringLength(100)]
        public string Ad { get; set; }
        
        public string Aciklama { get; set; }
        
        [StringLength(7)]
        public string Renk { get; set; } = "#3B82F6";
        
        // Navigation property
        public ICollection<Proje> Projeler { get; set; }
    }
} 