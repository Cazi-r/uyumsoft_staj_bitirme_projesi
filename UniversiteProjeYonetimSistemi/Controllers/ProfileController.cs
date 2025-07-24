using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public ProfileController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var kullanici = await _authService.GetCurrentUserAsync();
            if (kullanici == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Id = kullanici.Id,
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Email = kullanici.Email,
                Rol = kullanici.Rol,
                SonGiris = kullanici.SonGiris,
                CreatedAt = kullanici.CreatedAt
            };

            // Kullanıcı rolüne göre ek bilgileri yükle
            switch (kullanici.Rol)
            {
                case "Ogrenci":
                    var ogrenci = await _context.Ogrenciler
                        .FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                    
                    if (ogrenci != null)
                    {
                        model.OgrenciNo = ogrenci.OgrenciNo;
                        model.Telefon = ogrenci.Telefon;
                        model.Adres = ogrenci.Adres;
                        model.KayitTarihi = ogrenci.KayitTarihi;
                    }
                    break;

                case "Akademisyen":
                    var akademisyen = await _context.Akademisyenler
                        .FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                    
                    if (akademisyen != null)
                    {
                        model.Unvan = akademisyen.Unvan;
                        model.UzmanlikAlani = akademisyen.UzmanlikAlani;
                        model.Ofis = akademisyen.Ofis;
                        model.Telefon = akademisyen.Telefon;
                    }
                    break;
            }

            return View(model);
        }

        public async Task<IActionResult> Edit()
        {
            var kullanici = await _authService.GetCurrentUserAsync();
            if (kullanici == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = kullanici.Id,
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Email = kullanici.Email,
                Rol = kullanici.Rol
            };

            // Kullanıcı rolüne göre ek bilgileri yükle
            switch (kullanici.Rol)
            {
                case "Ogrenci":
                    var ogrenci = await _context.Ogrenciler
                        .FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                    
                    if (ogrenci != null)
                    {
                        model.OgrenciNo = ogrenci.OgrenciNo;
                        model.Telefon = ogrenci.Telefon;
                        model.Adres = ogrenci.Adres;
                    }
                    break;

                case "Akademisyen":
                    var akademisyen = await _context.Akademisyenler
                        .FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                    
                    if (akademisyen != null)
                    {
                        model.Unvan = akademisyen.Unvan;
                        model.UzmanlikAlani = akademisyen.UzmanlikAlani;
                        model.Ofis = akademisyen.Ofis;
                        model.Telefon = akademisyen.Telefon;
                    }
                    break;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = await _context.Kullanicilar.FindAsync(model.Id);
                if (kullanici == null)
                {
                    return NotFound();
                }

                // Başka bir kullanıcının e-posta adresini kullanıyor mu kontrol et
                if (kullanici.Email != model.Email)
                {
                    if (await _context.Kullanicilar.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                        return View(model);
                    }
                }

                // Kullanıcı bilgilerini güncelle
                kullanici.Ad = model.Ad;
                kullanici.Soyad = model.Soyad;
                kullanici.Email = model.Email;
                kullanici.UpdatedAt = DateTime.Now;

                // Şifre değişikliği isteği varsa güncelle
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    kullanici.Sifre = _authService.HashPassword(model.NewPassword);
                }

                // Role göre ek bilgileri güncelle
                switch (kullanici.Rol)
                {
                    case "Ogrenci":
                        var ogrenci = await _context.Ogrenciler
                            .FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                        
                        if (ogrenci != null)
                        {
                            ogrenci.Ad = model.Ad;
                            ogrenci.Soyad = model.Soyad;
                            ogrenci.Email = model.Email;
                            ogrenci.Telefon = model.Telefon ?? "";
                            ogrenci.Adres = model.Adres ?? "";
                            ogrenci.UpdatedAt = DateTime.Now;
                        }
                        break;

                    case "Akademisyen":
                        var akademisyen = await _context.Akademisyenler
                            .FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                        
                        if (akademisyen != null)
                        {
                            akademisyen.Ad = model.Ad;
                            akademisyen.Soyad = model.Soyad;
                            akademisyen.Email = model.Email;
                            akademisyen.Unvan = model.Unvan ?? "";
                            akademisyen.UzmanlikAlani = model.UzmanlikAlani ?? "";
                            akademisyen.Ofis = model.Ofis ?? "";
                            akademisyen.Telefon = model.Telefon ?? "";
                            akademisyen.UpdatedAt = DateTime.Now;
                        }
                        break;
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError(string.Empty, "Güncelleme sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                }
            }

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult Ayarlar()
        {
            ViewData["Title"] = "Görünüm Ayarları";
            return View();
        }
    }
} 