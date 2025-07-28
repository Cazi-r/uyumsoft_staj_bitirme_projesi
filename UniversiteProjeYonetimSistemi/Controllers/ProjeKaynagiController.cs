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

        // GET: ProjeKaynagi/Create
        public async Task<IActionResult> Create(int projeId)
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

        // POST: ProjeKaynagi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjeKaynagiViewModel model)
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
    }
}