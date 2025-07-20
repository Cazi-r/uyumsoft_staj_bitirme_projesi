using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public interface IProjeService
    {
        Task<IEnumerable<Proje>> GetAllAsync();
        Task<Proje> GetByIdAsync(int id);
        Task<IEnumerable<Proje>> GetByOgrenciIdAsync(int ogrenciId);
        Task<IEnumerable<Proje>> GetByMentorIdAsync(int mentorId);
        Task<IEnumerable<Proje>> GetByKategoriIdAsync(int kategoriId);
        Task<IEnumerable<Proje>> GetByStatusAsync(string status);
        Task<Proje> AddAsync(Proje proje);
        Task UpdateAsync(Proje proje);
        Task DeleteAsync(int id);
        Task AssignToOgrenciAsync(int projeId, int ogrenciId);
        Task AssignToMentorAsync(int projeId, int mentorId);
        Task UpdateStatusAsync(int projeId, string newStatus);
    }
} 