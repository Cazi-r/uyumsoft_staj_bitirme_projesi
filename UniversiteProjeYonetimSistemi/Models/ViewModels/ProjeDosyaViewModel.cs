using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace UniversiteProjeYonetimSistemi.Models.ViewModels
{
    public class ProjeDosyaViewModel
    {
        public int ProjeId { get; set; }
        
        [Required(ErrorMessage = "Lütfen bir dosya seçin.")]
        [Display(Name = "Dosya")]
        [MaxFileSize(50 * 1024 * 1024, ErrorMessage = "Dosya boyutu 50MB'dan büyük olamaz.")]
        [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", 
            ".jpg", ".jpeg", ".png", ".gif", ".xlsx", ".xls", ".ppt", ".pptx" }, 
            ErrorMessage = "Bu dosya türü desteklenmiyor.")]
        public IFormFile Dosya { get; set; }
        
        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
        public string Aciklama { get; set; }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Dosya boyutu {_maxFileSize / 1024 / 1024}MB'dan büyük olamaz.";
        }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Bu dosya türü desteklenmiyor. Desteklenen türler: {string.Join(", ", _extensions)}";
        }
    }
} 