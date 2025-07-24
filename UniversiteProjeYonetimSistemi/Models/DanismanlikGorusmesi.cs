using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    public class DanismanlikGorusmesi : TemelVarlik
    {
        // Id TemelVarlik'tan geliyor, tekrar tanımlamaya gerek yok
        
        [Required]
        [Display(Name = "Proje")]
        public int ProjeId { get; set; }
        
        [ForeignKey("ProjeId")]
        public virtual Proje Proje { get; set; }
        
        [Required]
        [Display(Name = "Akademisyen")]
        public int AkademisyenId { get; set; }
        
        [ForeignKey("AkademisyenId")]
        public virtual Akademisyen Akademisyen { get; set; }
        
        [Required]
        [Display(Name = "Öğrenci")]
        public int OgrenciId { get; set; }
        
        [ForeignKey("OgrenciId")]
        public virtual Ogrenci Ogrenci { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Başlık")]
        public string Baslik { get; set; }
        
        [Required]
        [Display(Name = "Görüşme Tarihi")]
        public DateTime GorusmeTarihi { get; set; }
        
        [Required]
        [Display(Name = "Görüşme Tipi")]
        public string GorusmeTipi { get; set; } // Online, Yüz Yüze
        
        [Display(Name = "Notlar")]
        public string Notlar { get; set; }
        
        [Required]
        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Beklemede"; // Beklemede, Onaylandı, Reddedildi
        
        [Display(Name = "Talep Eden")]
        public string TalepEden { get; set; } // "Ogrenci" veya "Akademisyen"

        [Display(Name = "Zaman Durumu")]
        public string ZamanDurumu { get; set; } // "Bugun", "YakinGelecek", "UzakGelecek", "Gecmis"
        
        // ZamanDurumu değerini otomatik ayarlayan metot
        public void GuncelleZamanDurumu()
        {
            var bugun = DateTime.Today;
            var birHaftaSonra = bugun.AddDays(7);
            
            if (GorusmeTarihi.Date == bugun)
            {
                ZamanDurumu = "Bugun";
            }
            else if (GorusmeTarihi.Date > bugun && GorusmeTarihi.Date <= birHaftaSonra)
            {
                ZamanDurumu = "YakinGelecek";
            }
            else if (GorusmeTarihi.Date > birHaftaSonra)
            {
                ZamanDurumu = "UzakGelecek";
            }
            else
            {
                ZamanDurumu = "Gecmis";
            }
        }
    }
} 