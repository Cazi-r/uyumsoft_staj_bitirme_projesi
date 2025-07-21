using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class DegerlendirmeViewModel
    {
        public int ProjeId { get; set; }
        
        [Required(ErrorMessage = "Puan zorunludur.")]
        [Range(0, 100, ErrorMessage = "Puan 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "Puan")]
        public int Puan { get; set; }
        
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }
        
        [Required(ErrorMessage = "Değerlendirme tipi seçilmelidir.")]
        [Display(Name = "Değerlendirme Tipi")]
        public string DegerlendirmeTipi { get; set; } = "Genel";
    }
} 