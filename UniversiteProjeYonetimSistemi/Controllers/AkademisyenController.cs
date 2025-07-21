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

        // Akademisyenin danışmanlığını yaptığı projeleri listeler
        public async Task<IActionResult> Danismanliklar(string durum = "")
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
            
            // Durum filtresi uygula
            if (!string.IsNullOrEmpty(durum))
            {
                projeler = projeler.Where(p => p.Status == durum);
            }
            
            // Proje durum bazlı istatistikler
            ViewBag.TumProjeSayisi = projeler.Count();
            ViewBag.BeklemedeProjeSayisi = projeler.Count(p => p.Status == "Beklemede");
            ViewBag.DevamEdenProjeSayisi = projeler.Count(p => p.Status == "Devam");
            ViewBag.TamamlananProjeSayisi = projeler.Count(p => p.Status == "Tamamlandi");
            ViewBag.SecilenDurum = durum;

            // Son eklenen değerlendirmeleri getir
            var sonDegerlendirmeler = await _akademisyenService.GetDegerlendirmelerAsync(akademisyen.Id);
            ViewBag.SonDegerlendirmeler = sonDegerlendirmeler
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToList();

            // Yaklaşan teslim tarihi olan projeleri getir
            var bugun = DateTime.Today;
            var ikiHaftaSonra = bugun.AddDays(14);
            
            ViewBag.YaklasanTeslimler = projeler
                .Where(p => p.TeslimTarihi.HasValue && 
                           p.TeslimTarihi >= bugun && 
                           p.TeslimTarihi <= ikiHaftaSonra)
                .OrderBy(p => p.TeslimTarihi)
                .ToList();

            return View(projeler.ToList());
        }

        // Proje detaylarını gösterir
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

        // Değerlendirme oluşturma sayfasını gösterir
        public async Task<IActionResult> DegerlendirmeOlustur(int projeId)
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
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null)
            {
                return NotFound("Proje bulunamadı.");
            }

            // Akademisyen bu projenin danışmanı mı kontrol et
            if (proje.MentorId != akademisyen.Id)
            {
                return Forbid("Bu projeyi değerlendirme yetkiniz bulunmamaktadır.");
            }

            ViewBag.Proje = proje;
            return View();
        }

        // Değerlendirme oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DegerlendirmeOlustur(int projeId, int puan, string aciklama, string degerlendirmeTipi)
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
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null)
            {
                return NotFound("Proje bulunamadı.");
            }

            // Akademisyen bu projenin danışmanı mı kontrol et
            if (proje.MentorId != akademisyen.Id)
            {
                return Forbid("Bu projeyi değerlendirme yetkiniz bulunmamaktadır.");
            }

            // Değerlendirme oluştur
            await _projeService.AddEvaluationAsync(projeId, puan, aciklama, degerlendirmeTipi, akademisyen.Id);
            
            return RedirectToAction("ProjeDetay", new { id = projeId });
        }
        
        // Yorum ekleme 
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