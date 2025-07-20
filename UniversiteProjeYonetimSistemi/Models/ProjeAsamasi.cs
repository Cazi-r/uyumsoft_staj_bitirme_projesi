using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class ProjeAsamasi : TemelVarlik
    {
        [ForeignKey("Proje")]
        public int ProjeId { get; set; }
        public Proje Proje { get; set; }
        
        [Required]
        [StringLength(100)]
        public string AsamaAdi { get; set; }
        
        public string Aciklama { get; set; }
        
        public DateTime? BaslangicTarihi { get; set; }
        
        public DateTime? BitisTarihi { get; set; }
        
        public bool Tamamlandi { get; set; } = false;
        
        public DateTime? TamamlanmaTarihi { get; set; }
        
        [Required]
        public int SiraNo { get; set; }
    }
} 