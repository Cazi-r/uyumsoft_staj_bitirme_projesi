using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        
        // Dosya işlemleri
        Task<ProjeDosya> UploadFileAsync(int projeId, IFormFile file, string aciklama, int? yukleyenId, string yukleyenTipi);
        Task<ProjeDosya> GetFileByIdAsync(int fileId);
        Task DeleteFileAsync(int fileId);
        Task<IEnumerable<ProjeDosya>> GetFilesByProjeIdAsync(int projeId);
        
        // Yorum işlemleri
        Task<ProjeYorum> AddCommentAsync(int projeId, string icerik, string yorumTipi, int? ogrenciId, int? akademisyenId);
        Task<IEnumerable<ProjeYorum>> GetCommentsByProjeIdAsync(int projeId);
        Task DeleteCommentAsync(int yorumId);
        
        // Değerlendirme işlemleri
        Task<Degerlendirme> AddEvaluationAsync(int projeId, int puan, string aciklama, string degerlendirmeTipi, int akademisyenId);
        Task<IEnumerable<Degerlendirme>> GetEvaluationsByProjeIdAsync(int projeId);
        Task UpdateEvaluationAsync(int degerlendirmeId, int puan, string aciklama, string degerlendirmeTipi);
        Task DeleteEvaluationAsync(int degerlendirmeId);
        
        // Proje aşamaları işlemleri
        Task<ProjeAsamasi> AddStageAsync(int projeId, string asamaAdi, string aciklama, DateTime? baslangicTarihi, DateTime? bitisTarihi, int siraNo);
        Task<IEnumerable<ProjeAsamasi>> GetStagesByProjeIdAsync(int projeId);
        Task UpdateStageAsync(int asamaId, string asamaAdi, string aciklama, DateTime? baslangicTarihi, DateTime? bitisTarihi, int siraNo, bool tamamlandi);
        Task UpdateStageStatusAsync(int asamaId, bool tamamlandi);
        Task DeleteStageAsync(int asamaId);
        Task<int> GetDegerlendirilmeyenAsamaSayisiByMentorIdAsync(int mentorId);
        Task UpdateStageEvaluatedStatusAsync(int asamaId, bool degerlendirildi);
        
        // Proje kaynakları işlemleri
        Task<ProjeKaynagi> AddResourceAsync(int projeId, string kaynakAdi, string kaynakTipi, string url, string aciklama, string yazar, DateTime? yayinTarihi);
        Task<IEnumerable<ProjeKaynagi>> GetResourcesByProjeIdAsync(int projeId);
        Task<ProjeKaynagi> GetResourceByIdAsync(int kaynakId);
        Task UpdateResourceAsync(ProjeKaynagi kaynak);
        Task DeleteResourceAsync(int kaynakId);
    }
} 