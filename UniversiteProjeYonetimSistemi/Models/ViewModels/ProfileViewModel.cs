using System;
using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Son Giriş")]
        public DateTime? SonGiris { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; }

        // Öğrenci özellikleri
        [Display(Name = "Öğrenci Numarası")]
        public string OgrenciNo { get; set; }

        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [Display(Name = "Telefon")]
        public string Telefon { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime? KayitTarihi { get; set; }

        // Akademisyen özellikleri
        [Display(Name = "Unvan")]
        public string Unvan { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; }

        [Display(Name = "Ofis")]
        public string Ofis { get; set; }
    }
} 