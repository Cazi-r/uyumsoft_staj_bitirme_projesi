using System;

namespace UniversiteProjeYonetimSistemi.Models
{
    public abstract class TemelVarlik
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
} 