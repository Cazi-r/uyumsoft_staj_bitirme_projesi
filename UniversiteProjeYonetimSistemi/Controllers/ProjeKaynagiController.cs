using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using System;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using System.Collections.Generic;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeKaynagiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;

        public ProjeKaynagiController(
            ApplicationDbContext context,
            IProjeService projeService,
            AuthService authService)
        {
            _context = context;
            _projeService = projeService;
            _authService = authService;
        }

        private async Task<bool> HasProjectPermission(int projeId)
        {
            var proje = await _context.Projeler
                .Include(p => p.Mentor)
                .Include(p => p.Ogrenci)
                .FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje == null) return false;
            if (User.IsInRole("Admin")) return true;

            if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                return akademisyen != null && proje.MentorId == akademisyen.Id;
            }

            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                return ogrenci != null && proje.OgrenciId == ogrenci.Id;
            }

            return false;
        }

        // Projeye kaynak ekleme formu: Erişim izni Admin/danisman/ogrenci-sahip kombinasyonuna gore kontrol edilir.
        public async Task<IActionResult> Add(int projeId)
        {
            if (!await HasProjectPermission(projeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null)
            {
                return NotFound();
            }

            var model = new ProjeKaynagiViewModel
            {
                ProjeId = projeId
            };

            ViewBag.ProjeAdi = proje.Ad;
            ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
            return View(model);
        }

        // Kaynak ekleme POST: form hatalari kontrol edilir; izni olan kullanici icin kayit olusturulur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProjeKaynagiViewModel model)
        {
            if (!await HasProjectPermission(model.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            // Debug: ModelState hatalarını kontrol et
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Form hataları: " + string.Join(", ", errors);
                ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
                return View(model);
            }

            try
            {
                var projeKaynagi = new ProjeKaynagi
                {
                    ProjeId = model.ProjeId,
                    KaynakAdi = model.KaynakAdi,
                    KaynakTipi = model.KaynakTipi,
                    Url = model.Url,
                    Yazar = model.Yazar,
                    YayinTarihi = model.YayinTarihi,
                    Aciklama = model.Aciklama,
                    EklemeTarihi = DateTime.Now
                };

                _context.ProjeKaynaklari.Add(projeKaynagi);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Kaynak başarıyla eklendi.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kaynak eklenirken bir hata oluştu: " + ex.Message;
                // Debug: Exception detaylarını logla
                System.Diagnostics.Debug.WriteLine($"ProjeKaynagi Create Error: {ex}");
            }

            ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
            return View(model);
        }

        // Projeye ait tum kaynaklari listeler; erisim yetkisi kontrol edilir.
        public async Task<IActionResult> Index(int projeId)
        {
            if (!await HasProjectPermission(projeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            var kaynaklar = await _projeService.GetResourcesByProjeIdAsync(projeId);
            return View(kaynaklar);
        }

        // Tek kaynak detayini gosterir; proje erisim yetkisi olmayan kullanicilar icin engeller.
        public async Task<IActionResult> Details(int id)
        {
            var kaynak = await _projeService.GetResourceByIdAsync(id);
            if (kaynak == null)
            {
                return NotFound();
            }

            if (!await HasProjectPermission(kaynak.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = kaynak.ProjeId });
            }

            return View(kaynak);
        }

        // Kaynak duzenleme formu: Erişim izni kontrol edilir ve model doldurulur.
        public async Task<IActionResult> Edit(int id)
        {
            var kaynak = await _projeService.GetResourceByIdAsync(id);
            if (kaynak == null)
            {
                return NotFound();
            }

            if (!await HasProjectPermission(kaynak.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = kaynak.ProjeId });
            }

            var model = new ProjeKaynagiViewModel
            {
                Id = kaynak.Id,
                ProjeId = kaynak.ProjeId,
                KaynakAdi = kaynak.KaynakAdi,
                KaynakTipi = kaynak.KaynakTipi,
                Url = kaynak.Url,
                Yazar = kaynak.Yazar,
                YayinTarihi = kaynak.YayinTarihi,
                Aciklama = kaynak.Aciklama
            };

            ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
            return View(model);
        }

        // Kaynak duzenleme POST: model dogrulanir ve degisiklikler kaydedilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjeKaynagiViewModel model)
        {
            if (!await HasProjectPermission(model.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
                return View(model);
            }

            try
            {
                var kaynak = await _projeService.GetResourceByIdAsync(model.Id);
                if (kaynak == null)
                {
                    return NotFound();
                }

                kaynak.KaynakAdi = model.KaynakAdi;
                kaynak.KaynakTipi = model.KaynakTipi;
                kaynak.Url = model.Url;
                kaynak.Yazar = model.Yazar;
                kaynak.YayinTarihi = model.YayinTarihi;
                kaynak.Aciklama = model.Aciklama;
                kaynak.UpdatedAt = DateTime.Now;

                await _projeService.UpdateResourceAsync(kaynak);
                
                TempData["SuccessMessage"] = "Kaynak başarıyla güncellendi.";
                return RedirectToAction("Index", new { projeId = model.ProjeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kaynak güncellenirken bir hata oluştu: " + ex.Message;
            }

            ViewBag.KaynakTipleri = new List<string> { "Kitap", "Makale", "Website", "API", "Dokuman", "Video", "Diger" };
            return View(model);
        }

        // Kaynak silme onayi: Erişim izni kontrol edilir; kaynak varligi dogrulanir.
        public async Task<IActionResult> Delete(int id)
        {
            var kaynak = await _projeService.GetResourceByIdAsync(id);
            if (kaynak == null)
            {
                return NotFound();
            }

            if (!await HasProjectPermission(kaynak.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = kaynak.ProjeId });
            }

            return View(kaynak);
        }

        // Kaynak silme POST: izni olan kullanici icin kaydi siler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            if (!await HasProjectPermission(projeId))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            try
            {
                await _projeService.DeleteResourceAsync(id);
                TempData["SuccessMessage"] = "Kaynak başarıyla silindi.";
                return RedirectToAction("Index", new { projeId = projeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kaynak silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", new { projeId = projeId });
            }
        }
    }
}