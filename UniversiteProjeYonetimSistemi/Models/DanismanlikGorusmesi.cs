using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversiteProjeYonetimSistemi.Models
{
    // Gorusme durumlarini net bir sekilde tanimlamak icin enum kullanalim.
    public enum GorusmeDurumu
    {
        [Display(Name = "Hoca Onayı Bekliyor")]
        HocaOnayiBekliyor, // Öğrenci talep oluşturduğunda veya yeni tarih önerdiğinde

        [Display(Name = "Öğrenci Onayı Bekliyor")]
        OgrenciOnayiBekliyor, // Hoca yeni tarih önerdiğinde

        [Display(Name = "Onaylandı")]
        Onaylandi, // İki taraf da tarihi kabul ettiğinde

        [Display(Name = "İptal Edildi")]
        IptalEdildi, // Taraflardan biri iptal ettiğinde

        [Display(Name = "Gerçekleştirildi")]
        Gerceklestirildi // Görüşme zamanı geçtiğinde ve onaylandığında
    }

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
        public GorusmeDurumu Durum { get; set; } // Enum tipi olarak degistirildi.
        
        [Display(Name = "Talep Eden")]
        public string TalepEden { get; set; } // "Ogrenci" veya "Akademisyen" - İlk talebi kimin yaptığını tutar
        
        // Bu yeni alan, en son kimin tarih değişikliği yaptığını tutacak.
        // Böylece sıranın kimde olduğunu bileceğiz.
        [Display(Name = "Son Güncelleyen Rolü")]
        public string SonGuncelleyenRol { get; set; } // "Ogrenci" veya "Akademisyen"

        [Display(Name = "Zaman Durumu")]
        public string ZamanDurumu { get; set; } // "Bugun", "YakinGelecek", "UzakGelecek", "Gecmis"
        
        // ZamanDurumu değerini otomatik ayarlayan metot
        public void GuncelleZamanDurumu()
        {
            // Sadece onaylanmış ve zamanı geçmiş görüşmelerin durumunu 'Gerçekleştirildi' olarak güncelle.
            if (Durum == GorusmeDurumu.Onaylandi && GorusmeTarihi < DateTime.Now)
            {
                Durum = GorusmeDurumu.Gerceklestirildi;
            }

            // Görüşmenin zamanına göre etiket belirle (UI için).
            var bugun = DateTime.Today;
            var birHaftaSonra = bugun.AddDays(7);
            
            if (GorusmeTarihi.Date == bugun)
            {
                ZamanDurumu = "Bugün";
            }
            else if (GorusmeTarihi.Date > bugun && GorusmeTarihi.Date <= birHaftaSonra)
            {
                ZamanDurumu = "Yaklaşan";
            }
            else
            {
                ZamanDurumu = "İleri Tarihli";
            }
        }
    }
} 