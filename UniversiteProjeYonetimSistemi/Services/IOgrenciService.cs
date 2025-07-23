using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public interface IOgrenciService
    {
        Task<IEnumerable<Ogrenci>> GetAllAsync();
        Task<Ogrenci> GetByIdAsync(int id);
        Task<Ogrenci> GetByKullaniciIdAsync(int kullaniciId);
        Task<Ogrenci> GetByOgrenciNoAsync(string ogrenciNo);
        Task<Ogrenci> GetOgrenciByUserName(string userName);
        Task<Ogrenci> AddAsync(Ogrenci ogrenci);
        Task UpdateAsync(Ogrenci ogrenci);
        Task DeleteAsync(int id);
        Task<IEnumerable<Proje>> GetProjelerAsync(int ogrenciId);
    }
} 