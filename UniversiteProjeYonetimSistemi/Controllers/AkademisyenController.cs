using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Akademisyen")]
    public class AkademisyenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;

        public AkademisyenController(
            ApplicationDbContext context,
            IAkademisyenService akademisyenService,
            IProjeService projeService,
            AuthService authService)
        {
            _context = context;
            _akademisyenService = akademisyenService;
            _projeService = projeService;
            _authService = authService;
        }

        // Bu action, [Authorize(Roles = "Akademisyen")] kapsaminda; yalnizca Akademisyen roller erisebilir.
        // Akademisyenin danismanlik ettigi projeleri listeleyip filtre/istatistikleri hazirlar.
        public async Task<IActionResult> Danismanliklar(string durum = "", string kategori = "", string search = "")
        {
            // Giriş yapmış akademisyeni bul
            var kullanici = await _authService.GetCurrentUserAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var akademisyen = await _akademisyenService.GetByKullaniciIdAsync(kullanici.Id);
            if (akademisyen == null)
            {
                return NotFound("Akademisyen bilgileriniz bulunamadı.");
            }

            // Akademisyenin danışmanlık yaptığı projeleri getir
            var projeler = await _projeService.GetByMentorIdAsync(akademisyen.Id);
            
            // Filtreleri uygula
            if (!string.IsNullOrEmpty(durum))
            {
                projeler = projeler.Where(p => p.Status == durum);
            }
            
            if (!string.IsNullOrEmpty(kategori))
            {
                var kategoriId = int.Parse(kategori);
                projeler = projeler.Where(p => p.KategoriId == kategoriId);
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                projeler = projeler.Where(p => 
                    p.Ad.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (p.Ogrenci != null && (p.Ogrenci.Ad + " " + p.Ogrenci.Soyad).Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }
            
            // Proje durum bazlı istatistikler
            ViewBag.TumProjeSayisi = projeler.Count();
            ViewBag.BeklemedeProjeSayisi = projeler.Count(p => p.Status == "Beklemede");
            ViewBag.DevamEdenProjeSayisi = projeler.Count(p => p.Status == "Devam");
            ViewBag.TamamlananProjeSayisi = projeler.Count(p => p.Status == "Tamamlandi");
            ViewBag.SecilenDurum = durum;

            // Bekleyen değerlendirme sayısı
            ViewBag.BekleyenDegerlendirmeSayisi = projeler.Count(p => p.Status == "Devam");

            // Son eklenen değerlendirmeleri getir
            var sonDegerlendirmeler = await _akademisyenService.GetDegerlendirmelerAsync(akademisyen.Id);
            ViewBag.SonDegerlendirmeler = sonDegerlendirmeler
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToList();

            // Yaklaşan teslim tarihi olan projeleri getir (7 gün içinde)
            var bugun = DateTime.Today;
            var birHaftaSonra = bugun.AddDays(7);
            
            ViewBag.YaklasanTeslimler = projeler
                .Where(p => p.TeslimTarihi.HasValue && 
                           p.TeslimTarihi >= bugun && 
                           p.TeslimTarihi <= birHaftaSonra &&
                           p.Status != "Tamamlandi")
                .OrderBy(p => p.TeslimTarihi)
                .ToList();

            // Son yorumları getir
            var projeIds = projeler.Select(p => p.Id).ToList();
            var sonYorumlar = await _context.ProjeYorumlari
                .Include(y => y.Proje)
                .Include(y => y.Ogrenci)
                .Include(y => y.Akademisyen)
                .Where(y => projeIds.Contains(y.ProjeId))
                .OrderByDescending(y => y.OlusturmaTarihi)
                .Take(5)
                .ToListAsync();
            ViewBag.SonYorumlar = sonYorumlar;

            // Yaklaşan proje aşamalarını getir (7 gün içinde)
            var yaklasanAsamalar = await _context.ProjeAsamalari
                .Include(a => a.Proje)
                .Where(a => projeIds.Contains(a.ProjeId) && 
                           a.BitisTarihi.HasValue && 
                           a.BitisTarihi >= DateTime.Today && 
                           a.BitisTarihi <= DateTime.Today.AddDays(7) &&
                           !a.Tamamlandi)
                .OrderBy(a => a.BitisTarihi)
                .Take(5)
                .ToListAsync();
            ViewBag.YaklasanAsamalar = yaklasanAsamalar;
            ViewBag.YaklasanAsamaSayisi = yaklasanAsamalar.Count;

            // Kategorileri getir
            var kategoriler = await _context.ProjeKategorileri.ToListAsync();
            ViewBag.Kategoriler = kategoriler;

            return View(projeler.ToList());
        }

        // Bu action, yalnizca projenin danismani olan akademisyenin projeyi detayli gorebilmesi icindir.
        // Dosyalar, yorumlar, degerlendirmeler ve asamalari ViewBag ile doldurur.
        public async Task<IActionResult> ProjeDetay(int id)
        {
            // Giriş yapmış akademisyeni bul
            var kullanici = await _authService.GetCurrentUserAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var akademisyen = await _akademisyenService.GetByKullaniciIdAsync(kullanici.Id);
            if (akademisyen == null)
            {
                return NotFound("Akademisyen bilgileriniz bulunamadı.");
            }

            // Projeyi getir
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound("Proje bulunamadı.");
            }

            // Akademisyen bu projenin danışmanı mı kontrol et
            if (proje.MentorId != akademisyen.Id)
            {
                return Forbid("Bu projeyi görüntüleme yetkiniz bulunmamaktadır.");
            }

            // Proje dosyalarını getir
            var dosyalar = await _projeService.GetFilesByProjeIdAsync(id);
            ViewBag.Dosyalar = dosyalar;

            // Proje yorumlarını getir
            var yorumlar = await _projeService.GetCommentsByProjeIdAsync(id);
            ViewBag.Yorumlar = yorumlar;

            // Proje değerlendirmelerini getir
            var degerlendirmeler = await _projeService.GetEvaluationsByProjeIdAsync(id);
            ViewBag.Degerlendirmeler = degerlendirmeler;

            // Proje aşamalarını getir
            var asamalar = await _projeService.GetStagesByProjeIdAsync(id);
            ViewBag.Asamalar = asamalar;

            return View(proje);
        }


        
        // Projeye yorum ekler.
        // Yalnizca ilgili projenin danismani olan akademisyen yorum ekleyebilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YorumEkle(int projeId, string icerik, string yorumTipi)
        {
            // Giriş yapmış akademisyeni bul
            var kullanici = await _authService.GetCurrentUserAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var akademisyen = await _akademisyenService.GetByKullaniciIdAsync(kullanici.Id);
            if (akademisyen == null)
            {
                return NotFound("Akademisyen bilgileriniz bulunamadı.");
            }

            // Projeyi getir ve yetki kontrolü
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null || proje.MentorId != akademisyen.Id)
            {
                return Forbid("Bu projeye yorum yapma yetkiniz bulunmamaktadır.");
            }

            // Yorum ekle
            await _projeService.AddCommentAsync(projeId, icerik, yorumTipi, null, akademisyen.Id);
            
            return RedirectToAction("ProjeDetay", new { id = projeId });
        }
    }
} 