using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class BildirimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BildirimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [Authorize] ile giris zorunlu. Kullanici rolune gore (Ogrenci/Akademisyen) kendi bildirimlerini listeler.
        // tip ve okundu parametreleriyle filtre uygular, en yeni bildirimler ustte olacak sekilde siralar.
        public async Task<IActionResult> Index(string tip, bool? okundu)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id.ToString() == userId);

            if (kullanici == null)
                return NotFound();

            IQueryable<Bildirim> bildirimler = null;

            // Kullanıcı tipine göre bildirimleri getir
            if (kullanici.Rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                bildirimler = _context.Bildirimler.Where(b => b.OgrenciId == ogrenci.Id);
            }
            else if (kullanici.Rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                bildirimler = _context.Bildirimler.Where(b => b.AkademisyenId == akademisyen.Id);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            // Filtreleme: Bildirim tipine göre
            if (!string.IsNullOrEmpty(tip))
            {
                bildirimler = bildirimler.Where(b => b.BildirimTipi == tip);
            }

            // Filtreleme: Okundu durumuna göre
            if (okundu.HasValue)
            {
                bildirimler = bildirimler.Where(b => b.Okundu == okundu.Value);
            }

            // Sonuçları tarihe göre sırala (en yeniler önce)
            bildirimler = bildirimler.OrderByDescending(b => b.OlusturmaTarihi);

            ViewBag.BildirimTipleri = new[] { "Bilgi", "Uyari", "Hata", "Basari" };
            ViewBag.SecilenTip = tip;
            ViewBag.SecilenOkundu = okundu;

            return View(await bildirimler.ToListAsync());
        }

        // Bir bildirimi okundu/okunmadi olarak isaretler. Rol/kullanici yetkisi kontrol edilir.
        [HttpPost]
        public async Task<IActionResult> OkunduDurumu(int id, bool okundu)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id.ToString() == userId);

            if (kullanici == null)
                return NotFound();

            var bildirim = await _context.Bildirimler.FindAsync(id);

            if (bildirim == null)
                return NotFound();

            // Kullanıcının bu bildirimi görmeye yetkisi var mı kontrol et
            bool yetkili = false;
            if (kullanici.Rol == "Ogrenci" && bildirim.OgrenciId.HasValue)
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                yetkili = bildirim.OgrenciId == ogrenci.Id;
            }
            else if (kullanici.Rol == "Akademisyen" && bildirim.AkademisyenId.HasValue)
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                yetkili = bildirim.AkademisyenId == akademisyen.Id;
            }

            if (!yetkili)
                return Forbid();

            // Bildirimi güncelle
            bildirim.Okundu = okundu;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Aktif kullanicinin butun okunmamis bildirimlerini okundu yapar.
        // [HttpPost] ile tetiklenir; rol bazli filtreleme icinde calisir.
        [HttpPost]
        public async Task<IActionResult> TumunuOkunduYap()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id.ToString() == userId);

            if (kullanici == null)
                return NotFound();

            // Kullanıcı tipine göre bildirimleri getir
            if (kullanici.Rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                var bildirimler = await _context.Bildirimler
                    .Where(b => b.OgrenciId == ogrenci.Id && !b.Okundu)
                    .ToListAsync();

                foreach (var bildirim in bildirimler)
                {
                    bildirim.Okundu = true;
                }
            }
            else if (kullanici.Rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                var bildirimler = await _context.Bildirimler
                    .Where(b => b.AkademisyenId == akademisyen.Id && !b.Okundu)
                    .ToListAsync();

                foreach (var bildirim in bildirimler)
                {
                    bildirim.Okundu = true;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 