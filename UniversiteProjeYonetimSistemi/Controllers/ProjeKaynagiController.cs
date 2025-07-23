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
    [Authorize]
    public class ProjeKaynagiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjeService _projeService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IOgrenciService _ogrenciService;
        private readonly AuthService _authService;

        public ProjeKaynagiController(
            ApplicationDbContext context,
            IProjeService projeService,
            IAkademisyenService akademisyenService,
            IOgrenciService ogrenciService,
            AuthService authService)
        {
            _context = context;
            _projeService = projeService;
            _akademisyenService = akademisyenService;
            _ogrenciService = ogrenciService;
            _authService = authService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProjeKaynagi kaynak)
        {
            if (ModelState.IsValid)
            {
                var proje = await _context.Projeler
                    .Include(p => p.Mentor)
                    .Include(p => p.Ogrenci)
                    .FirstOrDefaultAsync(p => p.Id == kaynak.ProjeId);

                if (proje == null)
                {
                    return NotFound();
                }

                // Yetki kontrolü
                bool yetkiliMi = false;

                if (User.IsInRole("Admin"))
                {
                    yetkiliMi = true;
                }
                else if (User.IsInRole("Akademisyen"))
                {
                    var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                    if (akademisyen != null && proje.MentorId == akademisyen.Id)
                    {
                        yetkiliMi = true;
                    }
                }
                else if (User.IsInRole("Ogrenci"))
                {
                    var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                    if (ogrenci != null && proje.OgrenciId == ogrenci.Id)
                    {
                        yetkiliMi = true;
                    }
                }

                if (!yetkiliMi)
                {
                    return Forbid();
                }

                _context.Add(kaynak);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kaynak başarıyla eklendi.";
                return RedirectToAction("Details", "Proje", new { id = kaynak.ProjeId });
            }

            TempData["ErrorMessage"] = "Kaynak eklenirken bir hata oluştu.";
            return RedirectToAction("Details", "Proje", new { id = kaynak.ProjeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            var kaynak = await _context.ProjeKaynaklari.FindAsync(id);
            if (kaynak == null)
            {
                return NotFound();
            }

            var proje = await _context.Projeler
                .Include(p => p.Mentor)
                .FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje == null)
            {
                return NotFound();
            }

            // Yetki kontrolü
            bool yetkiliMi = false;

            if (User.IsInRole("Admin"))
            {
                yetkiliMi = true;
            }
            else if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                if (akademisyen != null && proje.MentorId == akademisyen.Id)
                {
                    yetkiliMi = true;
                }
            }
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                if (ogrenci != null && proje.OgrenciId == ogrenci.Id)
                {
                    yetkiliMi = true;
                }
            }

            if (!yetkiliMi)
            {
                return Forbid();
            }

            _context.ProjeKaynaklari.Remove(kaynak);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kaynak başarıyla silindi.";
            return RedirectToAction("Details", "Proje", new { id = projeId });
        }
    }
} 