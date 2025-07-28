using System;
using System.ComponentModel.DataAnnotations;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class ProjeKaynagiViewModel
    {
        public int ProjeId { get; set; }
        
        [Required(ErrorMessage = "Kaynak adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Kaynak adı en fazla {1} karakter olabilir.")]
        [Display(Name = "Kaynak Adı")]
        public string KaynakAdi { get; set; }
        
        [Required(ErrorMessage = "Kaynak türü seçmelisiniz.")]
        [Display(Name = "Kaynak Türü")]
        public string KaynakTipi { get; set; }
        
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [Display(Name = "URL")]
        public string Url { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
        public string Aciklama { get; set; }
        
        [StringLength(100, ErrorMessage = "Yazar adı en fazla {1} karakter olabilir.")]
        [Display(Name = "Yazar")]
        public string Yazar { get; set; }
        
        [Display(Name = "Yayın Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? YayinTarihi { get; set; }
    }
}
