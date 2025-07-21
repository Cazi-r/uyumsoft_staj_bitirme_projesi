using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class ProjeDosyaViewModel
    {
        public int ProjeId { get; set; }
        
        [Required(ErrorMessage = "Lütfen bir dosya seçin.")]
        [Display(Name = "Dosya")]
        public IFormFile Dosya { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(255, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
        public string Aciklama { get; set; }
    }
} 