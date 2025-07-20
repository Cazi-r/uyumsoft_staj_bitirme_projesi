using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public interface IAkademisyenService
    {
        Task<IEnumerable<Akademisyen>> GetAllAsync();
        Task<Akademisyen> GetByIdAsync(int id);
        Task<Akademisyen> GetByKullaniciIdAsync(int kullaniciId);
        Task<Akademisyen> AddAsync(Akademisyen akademisyen);
        Task UpdateAsync(Akademisyen akademisyen);
        Task DeleteAsync(int id);
        Task<IEnumerable<Proje>> GetProjelerAsync(int akademisyenId);
        Task<IEnumerable<Degerlendirme>> GetDegerlendirmelerAsync(int akademisyenId);
    }
} 