using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class ProjeYorumViewModel
    {
        public int ProjeId { get; set; }
        
        [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
        [Display(Name = "Yorum")]
        public string Icerik { get; set; }
        
        [Required(ErrorMessage = "Yorum tipi seçilmelidir.")]
        [Display(Name = "Yorum Tipi")]
        public string YorumTipi { get; set; } = "Genel";
    }
} 