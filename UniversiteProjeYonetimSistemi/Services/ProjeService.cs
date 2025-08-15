using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class ProjeService : IProjeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Proje> _projeRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjeService(
            ApplicationDbContext context, 
            IRepository<Proje> projeRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _projeRepository = projeRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        /// Tum projeleri iliskili Ogrenci, Mentor ve Kategori ile birlikte listeler.
        public async Task<IEnumerable<Proje>> GetAllAsync()
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .ToListAsync();
        }

        /// Id'ye gore projeyi iliskili tum detaylariyla (dosya, yorum, asama, kaynak, degerlendirme) birlikte dondurur.
        public async Task<Proje> GetByIdAsync(int id)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Include(p => p.Dosyalar)
                .Include(p => p.Yorumlar)
                    .ThenInclude(y => y.Ogrenci)
                .Include(p => p.Yorumlar)
                    .ThenInclude(y => y.Akademisyen)
                .Include(p => p.Degerlendirmeler)
                    .ThenInclude(d => d.Akademisyen)
                .Include(p => p.Asamalar)
                .Include(p => p.Kaynaklar)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// Ogrenci Id'ye gore projeleri dondurur.
        public async Task<IEnumerable<Proje>> GetByOgrenciIdAsync(int ogrenciId)
        {
            return await _context.Projeler
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Where(p => p.OgrenciId == ogrenciId)
                .ToListAsync();
        }

        /// Mentor Id'ye gore projeleri dondurur.
        public async Task<IEnumerable<Proje>> GetByMentorIdAsync(int mentorId)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Kategori)
                .Where(p => p.MentorId == mentorId)
                .ToListAsync();
        }

        /// Kategori Id'ye gore projeleri dondurur.
        public async Task<IEnumerable<Proje>> GetByKategoriIdAsync(int kategoriId)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Where(p => p.KategoriId == kategoriId)
                .ToListAsync();
        }

        /// Duruma gore projeleri dondurur (Beklemede/Devam/Tamamlandi/Iptal).
        public async Task<IEnumerable<Proje>> GetByStatusAsync(string status)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        /// Yeni proje olusturur ve olusturma tarihini isaretler.
        public async Task<Proje> AddAsync(Proje proje)
        {
            proje.OlusturmaTarihi = DateTime.Now;
            await _projeRepository.AddAsync(proje);
            return proje;
        }

        /// Proje kaydini gunceller.
        public async Task UpdateAsync(Proje proje)
        {
            await _projeRepository.UpdateAsync(proje);
        }

        /// Id'ye gore proje kaydini siler.
        public async Task DeleteAsync(int id)
        {
            await _projeRepository.DeleteAsync(id);
        }

        /// Projeyi belirtilen ogrenciye atar.
        public async Task AssignToOgrenciAsync(int projeId, int ogrenciId)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.OgrenciId = ogrenciId;
            await _projeRepository.UpdateAsync(proje);
        }

        /// Projeye belirtilen mentor'u atar.
        public async Task AssignToMentorAsync(int projeId, int mentorId)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.MentorId = mentorId;
            await _projeRepository.UpdateAsync(proje);
        }

        /// Proje durumunu verilen yeni durum degerine gunceller.
        public async Task UpdateStatusAsync(int projeId, string newStatus)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.Status = newStatus;
            await _projeRepository.UpdateAsync(proje);
        }

        // Dosya işlemleri
        /// Proje icin dosya yukler; fiziksel dosyayi kaydeder ve veritabanina dosya kaydi ekler.
        public async Task<ProjeDosya> UploadFileAsync(int projeId, IFormFile file, string aciklama, int? yukleyenId, string yukleyenTipi)
        {
            // Dosya yükleme klasörünü oluştur
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "projeler", projeId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            // Dosya adını güvenli hale getir ve benzersiz yap
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayı kaydet
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Dosya bilgilerini veritabanına kaydet
            var projeDosya = new ProjeDosya
            {
                ProjeId = projeId,
                DosyaAdi = file.FileName,
                DosyaYolu = "/uploads/projeler/" + projeId.ToString() + "/" + uniqueFileName,
                DosyaTipi = file.ContentType,
                DosyaBoyutu = file.Length,
                YuklemeTarihi = DateTime.Now,
                Aciklama = aciklama,
                YukleyenId = yukleyenId,
                YukleyenTipi = yukleyenTipi,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ProjeDosyalari.Add(projeDosya);
            await _context.SaveChangesAsync();

            return projeDosya;
        }

        /// 
        /// Dosya Id'sine gore dosya kaydini dondurur.
        /// 
        public async Task<ProjeDosya> GetFileByIdAsync(int fileId)
        {
            return await _context.ProjeDosyalari.FindAsync(fileId);
        }

        /// 
        /// Dosya Id'sine gore hem fiziksel dosyayi hem veritabanindaki kaydi siler.
        /// 
        public async Task DeleteFileAsync(int fileId)
        {
            var dosya = await _context.ProjeDosyalari.FindAsync(fileId);
            if (dosya != null)
            {
                // Fiziksel dosyayı sil
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, dosya.DosyaYolu.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Veritabanından kaydı sil
                _context.ProjeDosyalari.Remove(dosya);
                await _context.SaveChangesAsync();
            }
        }

        /// 
        /// Projeye ait tum dosyalari yuklenme tarihine gore dondurur.
        /// 
        public async Task<IEnumerable<ProjeDosya>> GetFilesByProjeIdAsync(int projeId)
        {
            return await _context.ProjeDosyalari
                .Where(d => d.ProjeId == projeId)
                .OrderByDescending(d => d.YuklemeTarihi)
                .ToListAsync();
        }

        // Yorum işlemleri
        /// 
        /// Projeye yorum ekler (Ogrenci/Akademisyen kaynakli) ve kaydi dondurur.
        /// 
        public async Task<ProjeYorum> AddCommentAsync(int projeId, string icerik, string yorumTipi, int? ogrenciId, int? akademisyenId)
        {
            var yorum = new ProjeYorum
            {
                ProjeId = projeId,
                Icerik = icerik,
                YorumTipi = yorumTipi,
                OlusturmaTarihi = DateTime.Now,
                OgrenciId = ogrenciId,
                AkademisyenId = akademisyenId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ProjeYorumlari.Add(yorum);
            await _context.SaveChangesAsync();

            return yorum;
        }

        /// 
        /// Projeye ait tum yorumlari, iliskili Ogrenci/Akademisyen ile birlikte tarihe gore dondurur.
        /// 
        public async Task<IEnumerable<ProjeYorum>> GetCommentsByProjeIdAsync(int projeId)
        {
            return await _context.ProjeYorumlari
                .Include(y => y.Ogrenci)
                .Include(y => y.Akademisyen)
                .Where(y => y.ProjeId == projeId)
                .OrderByDescending(y => y.OlusturmaTarihi)
                .ToListAsync();
        }



        /// 
        /// Yorum Id'sine gore proje yorumunu siler.
        /// 
        public async Task DeleteCommentAsync(int yorumId)
        {
            var yorum = await _context.ProjeYorumlari.FindAsync(yorumId);
            if (yorum != null)
            {
                _context.ProjeYorumlari.Remove(yorum);
                await _context.SaveChangesAsync();
            }
        }

        // Değerlendirme işlemleri
        /// 
        /// Projeye degerlendirme ekler ve kaydi dondurur.
        /// 
        public async Task<Degerlendirme> AddEvaluationAsync(int projeId, int puan, string aciklama, string degerlendirmeTipi, int akademisyenId)
        {
            var degerlendirme = new Degerlendirme
            {
                ProjeId = projeId,
                Puan = puan,
                Aciklama = aciklama,
                DegerlendirmeTipi = degerlendirmeTipi,
                DegerlendirmeTarihi = DateTime.Now,
                AkademisyenId = akademisyenId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Degerlendirmeler.Add(degerlendirme);
            await _context.SaveChangesAsync();

            return degerlendirme;
        }

        /// 
        /// Projeye ait tum degerlendirmeleri iliskili Akademisyen ile birlikte dondurur.
        /// 
        public async Task<IEnumerable<Degerlendirme>> GetEvaluationsByProjeIdAsync(int projeId)
        {
            return await _context.Degerlendirmeler
                .Include(d => d.Akademisyen)
                .Where(d => d.ProjeId == projeId)
                .OrderByDescending(d => d.DegerlendirmeTarihi)
                .ToListAsync();
        }

        /// 
        /// Degerlendirme Id'sine gore tek kaydi proje ve akademisyen bilgileriyle getirir.
        /// 
        public async Task<Degerlendirme> GetEvaluationByIdAsync(int degerlendirmeId)
        {
            return await _context.Degerlendirmeler
                .Include(d => d.Akademisyen)
                .Include(d => d.Proje)
                .FirstOrDefaultAsync(d => d.Id == degerlendirmeId);
        }

        /// 
        /// Degerlendirme kaydini gunceller (puan, aciklama, tip).
        /// 
        public async Task UpdateEvaluationAsync(int degerlendirmeId, int puan, string aciklama, string degerlendirmeTipi)
        {
            var degerlendirme = await _context.Degerlendirmeler.FindAsync(degerlendirmeId);
            if (degerlendirme != null)
            {
                degerlendirme.Puan = puan;
                degerlendirme.Aciklama = aciklama;
                degerlendirme.DegerlendirmeTipi = degerlendirmeTipi;
                degerlendirme.UpdatedAt = DateTime.Now;

                _context.Degerlendirmeler.Update(degerlendirme);
                await _context.SaveChangesAsync();
            }
        }

        /// 
        /// Degerlendirme Id'sine gore kaydi siler.
        /// 
        public async Task DeleteEvaluationAsync(int degerlendirmeId)
        {
            var degerlendirme = await _context.Degerlendirmeler.FindAsync(degerlendirmeId);
            if (degerlendirme != null)
            {
                _context.Degerlendirmeler.Remove(degerlendirme);
                await _context.SaveChangesAsync();
            }
        }

        // Proje aşamaları işlemleri
        /// 
        /// Projeye yeni asama ekler ve olusturma tarihlerini isaretler.
        /// 
        public async Task<ProjeAsamasi> AddStageAsync(int projeId, string asamaAdi, string aciklama, DateTime? baslangicTarihi, DateTime? bitisTarihi, int siraNo)
        {
            var asama = new ProjeAsamasi
            {
                ProjeId = projeId,
                AsamaAdi = asamaAdi,
                Aciklama = aciklama,
                BaslangicTarihi = baslangicTarihi,
                BitisTarihi = bitisTarihi,
                Tamamlandi = false,
                SiraNo = siraNo,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ProjeAsamalari.Add(asama);
            await _context.SaveChangesAsync();

            return asama;
        }

        /// 
        /// Projeye ait asamalari sira numarasina gore dondurur.
        /// 
        public async Task<IEnumerable<ProjeAsamasi>> GetStagesByProjeIdAsync(int projeId)
        {
            return await _context.ProjeAsamalari
                .Where(a => a.ProjeId == projeId)
                .OrderBy(a => a.SiraNo)
                .ToListAsync();
        }

        /// 
        /// Asama bilgilerini ve tamamlanma durumunu gunceller; tamamlandiysa tamamlanma tarihini isaretler.
        /// 
        public async Task UpdateStageAsync(int asamaId, string asamaAdi, string aciklama, DateTime? baslangicTarihi, DateTime? bitisTarihi, int siraNo, bool tamamlandi)
        {
            var asama = await _context.ProjeAsamalari.FindAsync(asamaId);
            if (asama != null)
            {
                asama.AsamaAdi = asamaAdi;
                asama.Aciklama = aciklama;
                asama.BaslangicTarihi = baslangicTarihi;
                asama.BitisTarihi = bitisTarihi;
                asama.SiraNo = siraNo;
                asama.Tamamlandi = tamamlandi;
                
                if (tamamlandi && !asama.TamamlanmaTarihi.HasValue)
                {
                    asama.TamamlanmaTarihi = DateTime.Now;
                }
                else if (!tamamlandi)
                {
                    asama.TamamlanmaTarihi = null;
                }
                
                asama.UpdatedAt = DateTime.Now;
                _context.ProjeAsamalari.Update(asama);
                await _context.SaveChangesAsync();
            }
        }

        /// 
        /// Sadece asama tamamlanma durumunu gunceller ve uygun tarih alanlarini ayarlar.
        /// 
        public async Task UpdateStageStatusAsync(int asamaId, bool tamamlandi)
        {
            var asama = await _context.ProjeAsamalari.FindAsync(asamaId);
            if (asama != null)
            {
                asama.Tamamlandi = tamamlandi;
                if (tamamlandi)
                {
                    asama.TamamlanmaTarihi = DateTime.Now;
                }
                else
                {
                    asama.TamamlanmaTarihi = null;
                }
                asama.UpdatedAt = DateTime.Now;

                _context.ProjeAsamalari.Update(asama);
                await _context.SaveChangesAsync();
            }
        }

        /// 
        /// Asama Id'sine gore asama kaydini siler.
        /// 
        public async Task DeleteStageAsync(int asamaId)
        {
            var asama = await _context.ProjeAsamalari.FindAsync(asamaId);
            if (asama != null)
            {
                _context.ProjeAsamalari.Remove(asama);
                await _context.SaveChangesAsync();
            }
        }
        
        /// 
        /// Belirtilen mentor icin tamamlanmis ancak degerlendirilmemis asama sayisini dondurur.
        /// 
        public async Task<int> GetDegerlendirilmeyenAsamaSayisiByMentorIdAsync(int mentorId)
        {
            return await _context.ProjeAsamalari
                .Include(a => a.Proje)
                .Where(a => a.Proje.MentorId == mentorId && a.Tamamlandi && !a.Degerlendirildi)
                .CountAsync();
        }
        
        /// 
        /// Asamanin degerlendirildi bilgisini ve ilgili tarih alanini gunceller.
        /// 
        public async Task UpdateStageEvaluatedStatusAsync(int asamaId, bool degerlendirildi)
        {
            var asama = await _context.ProjeAsamalari.FindAsync(asamaId);
            if (asama != null)
            {
                asama.Degerlendirildi = degerlendirildi;
                if (degerlendirildi)
                {
                    asama.DegerlendirmeTarihi = DateTime.Now;
                }
                else
                {
                    asama.DegerlendirmeTarihi = null;
                }
                asama.UpdatedAt = DateTime.Now;

                _context.ProjeAsamalari.Update(asama);
                await _context.SaveChangesAsync();
            }
        }

        // Proje Kaynakları işlemleri
        /// 
        /// Projeye yeni kaynak bilgisi ekler ve kaydi dondurur.
        /// 
        public async Task<ProjeKaynagi> AddResourceAsync(int projeId, string kaynakAdi, string kaynakTipi, string url, string aciklama, string yazar, DateTime? yayinTarihi)
        {
            var kaynak = new ProjeKaynagi
            {
                ProjeId = projeId,
                KaynakAdi = kaynakAdi,
                KaynakTipi = kaynakTipi,
                Url = url,
                Aciklama = aciklama,
                Yazar = yazar,
                YayinTarihi = yayinTarihi,
                EklemeTarihi = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ProjeKaynaklari.Add(kaynak);
            await _context.SaveChangesAsync();

            return kaynak;
        }

        /// 
        /// Projeye ait tum kaynaklari ekleme tarihine gore dondurur.
        /// 
        public async Task<IEnumerable<ProjeKaynagi>> GetResourcesByProjeIdAsync(int projeId)
        {
            return await _context.ProjeKaynaklari
                .Where(k => k.ProjeId == projeId)
                .OrderByDescending(k => k.EklemeTarihi)
                .ToListAsync();
        }

        /// 
        /// Kaynak Id'sine gore tek kaynak kaydini dondurur.
        /// 
        public async Task<ProjeKaynagi> GetResourceByIdAsync(int kaynakId)
        {
            return await _context.ProjeKaynaklari.FindAsync(kaynakId);
        }

        /// 
        /// Kaynak kaydini gunceller.
        /// 
        public async Task UpdateResourceAsync(ProjeKaynagi kaynak)
        {
            kaynak.UpdatedAt = DateTime.Now;
            _context.ProjeKaynaklari.Update(kaynak);
            await _context.SaveChangesAsync();
        }

        /// 
        /// Kaynak Id'sine gore kaynak kaydini siler.
        /// 
        public async Task DeleteResourceAsync(int kaynakId)
        {
            var kaynak = await _context.ProjeKaynaklari.FindAsync(kaynakId);
            if (kaynak != null)
            {
                _context.ProjeKaynaklari.Remove(kaynak);
                await _context.SaveChangesAsync();
            }
        }
    }
} 