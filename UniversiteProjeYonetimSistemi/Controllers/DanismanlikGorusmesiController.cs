using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class DanismanlikGorusmesiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOgrenciService _ogrenciService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IBildirimService _bildirimService;

        public DanismanlikGorusmesiController(
            ApplicationDbContext context, 
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IBildirimService bildirimService)
        {
            _context = context;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _bildirimService = bildirimService;
        }

        // Index: Basit görüşme listesi - sadece 3 kategori: Bekleyen, Onaylanmış, Geçmiş/İptal
        public async Task<IActionResult> Index()
        {
            var kullanici = HttpContext.User;
            
            if (kullanici.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(kullanici.Identity.Name);
                var gorusmeler = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Akademisyen)
                    .Include(d => d.Proje)
                    .Where(d => d.OgrenciId == ogrenci.Id)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                // Zaman durumlarını güncelle
                foreach (var gorusme in gorusmeler)
                {
                    gorusme.GuncelleZamanDurumu();
                }
                await _context.SaveChangesAsync();

                return View("OgrenciGorusmeleri", gorusmeler);
            }
            else if (kullanici.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(kullanici.Identity.Name);
                var gorusmeler = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Ogrenci)
                    .Include(d => d.Proje)
                    .Where(d => d.AkademisyenId == akademisyen.Id)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                // Zaman durumlarını güncelle
                foreach (var gorusme in gorusmeler)
                {
                    gorusme.GuncelleZamanDurumu();
                }
                await _context.SaveChangesAsync();
                
                return View("AkademisyenGorusmeleri", gorusmeler);
            }
            else if (kullanici.IsInRole("Admin"))
            {
                var gorusmeler = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Akademisyen)
                    .Include(d => d.Ogrenci)
                    .Include(d => d.Proje)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
                    
                return View(gorusmeler);
            }
            
            return Forbid();
        }

        // Details: Gorusme detaylarini gosterir; sadece alaka duyan kullanicilar (ogrenci/danisman/admin) gorebilir.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri
                .Include(d => d.Akademisyen)
                .Include(d => d.Ogrenci)
                .Include(d => d.Proje)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (danismanlikGorusmesi == null)
            {
                return NotFound();
            }

            // Authorization check
            if (!await CanAccessMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }

            return View(danismanlikGorusmesi);
        }

        // Create GET: Basit görüşme talebi formu
        [Authorize(Roles = "Ogrenci,Akademisyen")]
        public async Task<IActionResult> Create(int? projeId)
        {
            await PrepareViewDataForCreate(projeId);
            
            var gorusme = new DanismanlikGorusmesi();
            if (projeId.HasValue)
            {
                gorusme.ProjeId = projeId.Value;
                await SetProjectParticipants(gorusme, projeId.Value);
            }
            
            return View(gorusme);
        }

        // Create POST: Basit görüşme talebi oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci,Akademisyen")]
        public async Task<IActionResult> Create([Bind("ProjeId,AkademisyenId,OgrenciId,Baslik,GorusmeTarihi,GorusmeTipi,Notlar")] DanismanlikGorusmesi danismanlikGorusmesi)
        {
            if (ModelState.IsValid)
            {
                // Talep eden rolü belirle
                if (User.IsInRole("Ogrenci"))
                {
                    var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                    danismanlikGorusmesi.OgrenciId = ogrenci.Id;
                    danismanlikGorusmesi.TalepEden = "Ogrenci";
                    danismanlikGorusmesi.Durum = GorusmeDurumu.HocaOnayiBekliyor;
                }
                else if (User.IsInRole("Akademisyen"))
                {
                    var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                    danismanlikGorusmesi.AkademisyenId = akademisyen.Id;
                    danismanlikGorusmesi.TalepEden = "Akademisyen";
                    danismanlikGorusmesi.Durum = GorusmeDurumu.OgrenciOnayiBekliyor;
                }
                
                danismanlikGorusmesi.SonGuncelleyenRol = danismanlikGorusmesi.TalepEden;
                danismanlikGorusmesi.GuncelleZamanDurumu();
                
                _context.Add(danismanlikGorusmesi);
                await _context.SaveChangesAsync();
                
                // Bildirim gönder
                await _bildirimService.GorusmePlanlandiBildirimiGonder(danismanlikGorusmesi);
                
                TempData["Message"] = "Görüşme talebi başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareViewDataForCreate(danismanlikGorusmesi.ProjeId);
            return View(danismanlikGorusmesi);
        }

        // Edit GET: Sadece bekleyen talepler düzenlenebilir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri
                .Include(d => d.Proje)
                .Include(d => d.Akademisyen)
                .Include(d => d.Ogrenci)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (danismanlikGorusmesi == null)
            {
                return NotFound();
            }
            
            // Sadece bekleyen talepler düzenlenebilir
            if (danismanlikGorusmesi.Durum == GorusmeDurumu.Onaylandi || danismanlikGorusmesi.Durum == GorusmeDurumu.IptalEdildi)
            {
                TempData["Error"] = "Onaylanmış veya iptal edilmiş görüşmeler düzenlenemez.";
                return RedirectToAction(nameof(Index));
            }
            
            // Yetki kontrolü
            if (!await CanAccessMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }
            
            await PrepareViewDataForCreate(danismanlikGorusmesi.ProjeId);
            return View(danismanlikGorusmesi);
        }

        // Edit POST: Görüşme düzenle ve tekrar onaya gönder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjeId,AkademisyenId,OgrenciId,Baslik,GorusmeTarihi,GorusmeTipi,Notlar")] DanismanlikGorusmesi danismanlikGorusmesi)
        {
            if (id != danismanlikGorusmesi.Id)
            {
                return NotFound();
            }

            var mevcutGorusme = await _context.DanismanlikGorusmeleri
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (mevcutGorusme == null || !await CanAccessMeeting(mevcutGorusme))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut değerleri koru
                    danismanlikGorusmesi.TalepEden = mevcutGorusme.TalepEden;
                    danismanlikGorusmesi.CreatedAt = mevcutGorusme.CreatedAt;
                    
                    // Düzenleyen rolü güncelle
                    if (User.IsInRole("Ogrenci"))
                    {
                        danismanlikGorusmesi.Durum = GorusmeDurumu.HocaOnayiBekliyor;
                        danismanlikGorusmesi.SonGuncelleyenRol = "Ogrenci";
                    }
                    else if (User.IsInRole("Akademisyen"))
                    {
                        danismanlikGorusmesi.Durum = GorusmeDurumu.OgrenciOnayiBekliyor;
                        danismanlikGorusmesi.SonGuncelleyenRol = "Akademisyen";
                    }
                    
                    danismanlikGorusmesi.GuncelleZamanDurumu();
                    
                    _context.Update(danismanlikGorusmesi);
                    await _context.SaveChangesAsync();
                    
                    // Bildirim gönder
                    await _bildirimService.GorusmeDurumuDegistiBildirimiGonder(danismanlikGorusmesi);
                    
                    TempData["Message"] = "Görüşme talebi güncellendi ve tekrar onaya gönderildi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanismanlikGorusmesiExists(danismanlikGorusmesi.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            await PrepareViewDataForCreate(danismanlikGorusmesi.ProjeId);
            return View(danismanlikGorusmesi);
        }

        // Delete GET: Silme onayi; sadece erisim izni olan kullanicilar gorebilir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri
                .Include(d => d.Akademisyen)
                .Include(d => d.Ogrenci)
                .Include(d => d.Proje)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (danismanlikGorusmesi == null)
            {
                return NotFound();
            }
            
            if (!await CanAccessMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }

            return View(danismanlikGorusmesi);
        }

        // Delete POST: sadece erisim izni olan kullanicilar silebilir.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri.FindAsync(id);
            
            if (danismanlikGorusmesi == null || !await CanAccessMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }
            
            _context.DanismanlikGorusmeleri.Remove(danismanlikGorusmesi);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Basit onay/reddet sistemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Akademisyen,Ogrenci")]
        public async Task<IActionResult> OnayveyaReddet(int id, string karar)
        {
            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri
                .Include(d => d.Akademisyen)
                .Include(d => d.Ogrenci)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danismanlikGorusmesi == null)
            {
                return NotFound();
            }

            // Yetki kontrolü
            if (!await CanAccessMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }

            var guncelleyenRol = User.IsInRole("Ogrenci") ? "Ogrenci" : "Akademisyen";

            // Basit kontrol: sadece kendi sırası gelenlerin onaylayabilmesi
            bool islemGecerli = (guncelleyenRol == "Ogrenci" && danismanlikGorusmesi.Durum == GorusmeDurumu.OgrenciOnayiBekliyor) ||
                               (guncelleyenRol == "Akademisyen" && danismanlikGorusmesi.Durum == GorusmeDurumu.HocaOnayiBekliyor);

            if (!islemGecerli)
            {
                TempData["Error"] = "Bu işlemi şu anda gerçekleştiremezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            // Durumu güncelle
            if (karar == "Onayla")
            {
                danismanlikGorusmesi.Durum = GorusmeDurumu.Onaylandi;
                TempData["Message"] = "Görüşme talebi başarıyla onaylandı.";
            }
            else if (karar == "Reddet")
            {
                danismanlikGorusmesi.Durum = GorusmeDurumu.IptalEdildi;
                TempData["Message"] = "Görüşme talebi iptal edildi.";
            }
            else
            {
                TempData["Error"] = "Geçersiz işlem.";
                return RedirectToAction(nameof(Index));
            }

            danismanlikGorusmesi.SonGuncelleyenRol = guncelleyenRol;
            danismanlikGorusmesi.GuncelleZamanDurumu();
            
            _context.Update(danismanlikGorusmesi);
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _bildirimService.GorusmeDurumuDegistiBildirimiGonder(danismanlikGorusmesi);

            return RedirectToAction(nameof(Index));
        }

        #region Private Helper Methods

        // Görüşme oluşturma için gerekli ViewData hazırla
        private async Task PrepareViewDataForCreate(int? projeId)
        {
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                var projeler = await _context.Projeler
                    .Include(p => p.Mentor)
                    .Where(p => p.OgrenciId == ogrenci.Id)
                    .ToListAsync();
                
                // Akademisyenleri projeler üzerinden getir
                var akademisyenIds = projeler.Where(p => p.MentorId.HasValue).Select(p => p.MentorId.Value).Distinct();
                var akademisyenler = await _context.Akademisyenler
                    .Where(a => akademisyenIds.Contains(a.Id))
                    .ToListAsync();
                
                ViewData["ProjeId"] = new SelectList(projeler, "Id", "Ad", projeId);
                ViewData["AkademisyenId"] = new SelectList(akademisyenler.Select(a => new
                {
                    Id = a.Id,
                    TamAd = $"{a.Unvan} {a.Ad} {a.Soyad}"
                }), "Id", "TamAd");
                ViewData["UserRole"] = "Ogrenci";
            }
            else if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                var projeler = await _context.Projeler
                    .Include(p => p.Ogrenci)
                    .Where(p => p.MentorId == akademisyen.Id)
                    .ToListAsync();
                
                // Öğrencileri projeler üzerinden getir
                var ogrenciIds = projeler.Where(p => p.OgrenciId.HasValue).Select(p => p.OgrenciId.Value).Distinct();
                var ogrenciler = await _context.Ogrenciler
                    .Where(o => ogrenciIds.Contains(o.Id))
                    .ToListAsync();
                
                ViewData["ProjeId"] = new SelectList(projeler, "Id", "Ad", projeId);
                ViewData["OgrenciId"] = new SelectList(ogrenciler.Select(o => new
                {
                    Id = o.Id,
                    TamAd = $"{o.Ad} {o.Soyad} ({o.OgrenciNo})"
                }), "Id", "TamAd");
                ViewData["UserRole"] = "Akademisyen";
            }
        }

        // Proje seçildiğinde otomatik olarak öğrenci ve akademisyen bilgilerini ayarla
        private async Task SetProjectParticipants(DanismanlikGorusmesi gorusme, int projeId)
        {
            var proje = await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .FirstOrDefaultAsync(p => p.Id == projeId);
            
            if (proje != null)
            {
                gorusme.OgrenciId = proje.OgrenciId ?? 0;
                gorusme.AkademisyenId = proje.MentorId ?? 0;
                gorusme.Baslik = "Danışmanlık Görüşmesi";
            }
        }

        // Yetki kontrolü
        private async Task<bool> CanAccessMeeting(DanismanlikGorusmesi meeting)
        {
            if (User.IsInRole("Admin"))
                return true;
            
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                return meeting.OgrenciId == ogrenci.Id;
            }
            
            if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                return meeting.AkademisyenId == akademisyen.Id;
            }
            
            return false;
        }

        private bool DanismanlikGorusmesiExists(int id)
        {
            return _context.DanismanlikGorusmeleri.Any(e => e.Id == id);
        }
        #endregion
    }
}
