using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
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
        
        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az {2} ve en fazla {1} karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage = "Kullanıcı rolü seçilmelidir.")]
        [Display(Name = "Kullanıcı Türü")]
        public string Rol { get; set; }
        
        // Öğrenci için ek alanlar
        [Display(Name = "Öğrenci Numarası")]
        [StringLength(10, ErrorMessage = "Öğrenci numarası en fazla {1} karakter olabilir.")]
        public string OgrenciNo { get; set; }
        
        [Display(Name = "Adres")]
        public string Adres { get; set; }
        
        [Display(Name = "Telefon")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla {1} karakter olabilir.")]
        public string Telefon { get; set; }
        
        // Akademisyen için ek alanlar
        [Display(Name = "Unvan")]
        public string Unvan { get; set; }
        
        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; }
        
        [Display(Name = "Ofis")]
        public string Ofis { get; set; }
    }
} 