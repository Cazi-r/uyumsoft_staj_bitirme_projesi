using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla {1} karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla {1} karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla {1} karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; }

        // Şifre değiştirme (opsiyonel)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az {2} ve en fazla {1} karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Değiştirmek istemiyorsanız boş bırakın)")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        // Öğrenci özellikleri
        [Display(Name = "Öğrenci Numarası")]
        public string OgrenciNo { get; set; }

        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [Display(Name = "Telefon")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla {1} karakter olabilir.")]
        public string Telefon { get; set; }

        // Akademisyen özellikleri
        [Display(Name = "Unvan")]
        public string Unvan { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; }

        [Display(Name = "Ofis")]
        public string Ofis { get; set; }
    }
} 