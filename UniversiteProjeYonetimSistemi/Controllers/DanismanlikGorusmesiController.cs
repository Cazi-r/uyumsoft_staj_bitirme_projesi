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
        private readonly IProjeService _projeService;
        private readonly IOgrenciService _ogrenciService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IBildirimService _bildirimService;

        public DanismanlikGorusmesiController(
            ApplicationDbContext context, 
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IBildirimService bildirimService)
        {
            _context = context;
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _bildirimService = bildirimService;
        }

        // GET: DanismanlikGorusmesi
        public async Task<IActionResult> Index()
        {
            var kullanici = HttpContext.User;
            
            if (kullanici.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(kullanici.Identity.Name);
                var ogrenciGorusmeleri = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Akademisyen)
                    .Include(d => d.Proje)
                    .Where(d => d.OgrenciId == ogrenci.Id)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
                return View("OgrenciGorusmeleri", ogrenciGorusmeleri);
            }
            else if (kullanici.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(kullanici.Identity.Name);
                
                // Tüm görüşmeleri çek ve ZamanDurumu güncellemesini yap
                var akademisyenGorusmeleri = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Ogrenci)
                    .Include(d => d.Proje)
                    .Where(d => d.AkademisyenId == akademisyen.Id)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
                
                // Tüm görüşmelerin ZamanDurumu değerlerini güncelle
                foreach (var gorusme in akademisyenGorusmeleri)
                {
                    gorusme.GuncelleZamanDurumu();
                }
                
                // ViewBag'e farklı görüşme türlerini ekle
                ViewBag.BekleyenTalepler = akademisyenGorusmeleri
                    .Where(g => g.Durum == "Beklemede" && g.TalepEden == "Ogrenci")
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                
                ViewBag.YakinGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == "Onaylandı" && (g.ZamanDurumu == "Bugun" || g.ZamanDurumu == "YakinGelecek"))
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                    
                ViewBag.IleridekiGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == "Onaylandı" && g.ZamanDurumu == "UzakGelecek")
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                    
                ViewBag.GecmisGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == "Onaylandı" && g.ZamanDurumu == "Gecmis")
                    .OrderByDescending(g => g.GorusmeTarihi)
                    .ToList();
                
                ViewBag.IptalGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == "Reddedildi")
                    .OrderByDescending(g => g.GorusmeTarihi)
                    .ToList();
                
                return View("AkademisyenGorusmeleri", akademisyenGorusmeleri);
            }
            else if (kullanici.IsInRole("Admin"))
            {
                var tümGorusmeler = await _context.DanismanlikGorusmeleri
                    .Include(d => d.Akademisyen)
                    .Include(d => d.Ogrenci)
                    .Include(d => d.Proje)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
                return View(tümGorusmeler);
            }
            
            return Forbid();
        }

        // GET: DanismanlikGorusmesi/Details/5
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

        // GET: DanismanlikGorusmesi/Create
        [Authorize(Roles = "Ogrenci,Akademisyen")]
        public async Task<IActionResult> Create(int? projeId)
        {
            if (User.IsInRole("Ogrenci"))
            {
                return await CreateForStudent(projeId);
            }
            else if (User.IsInRole("Akademisyen"))
            {
                return await CreateForAcademician(projeId);
            }
            
            return Forbid();
        }

        // POST: DanismanlikGorusmesi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci,Akademisyen")]
        public async Task<IActionResult> Create([Bind("ProjeId,AkademisyenId,OgrenciId,Baslik,GorusmeTarihi,GorusmeTipi,Notlar")] DanismanlikGorusmesi danismanlikGorusmesi)
        {
            if (User.IsInRole("Ogrenci"))
            {
                return await CreatePostForStudent(danismanlikGorusmesi);
            }
            else if (User.IsInRole("Akademisyen"))
            {
                return await CreatePostForAcademician(danismanlikGorusmesi);
            }
            
            return Forbid();
        }

        // GET: DanismanlikGorusmesi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danismanlikGorusmesi = await _context.DanismanlikGorusmeleri.FindAsync(id);
            if (danismanlikGorusmesi == null)
            {
                return NotFound();
            }
            
            // Authorization and business logic checks
            if (!await CanEditMeeting(danismanlikGorusmesi))
            {
                return Forbid();
            }
            
            await PrepareEditViewData(danismanlikGorusmesi);
            return View(danismanlikGorusmesi);
        }

        // POST: DanismanlikGorusmesi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjeId,AkademisyenId,OgrenciId,Baslik,GorusmeTarihi,GorusmeTipi,Notlar,Durum")] DanismanlikGorusmesi danismanlikGorusmesi)
        {
            if (id != danismanlikGorusmesi.Id)
            {
                return NotFound();
            }

            var mevcutGorusme = await _context.DanismanlikGorusmeleri
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (mevcutGorusme == null || !await CanEditMeeting(mevcutGorusme))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateMeeting(danismanlikGorusmesi, mevcutGorusme);
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
            
            await PrepareEditViewData(danismanlikGorusmesi);
            return View(danismanlikGorusmesi);
        }

        // GET: DanismanlikGorusmesi/Delete/5
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

        // POST: DanismanlikGorusmesi/Delete/5
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

        // POST: DanismanlikGorusmesi/OnayveyaReddet
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Akademisyen,Ogrenci")]
        public async Task<IActionResult> OnayveyaReddet(int id, string durum)
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

            // Durumun geçerli bir değer olduğunu kontrol et
            if (durum != "Onaylandı" && durum != "Reddedildi")
            {
                TempData["Error"] = "Geçersiz durum değeri.";
                return RedirectToAction(nameof(Index));
            }

            // Sadece öğrenci, akademisyen taleplerini onaylayabilir/reddedebilir
            // Sadece akademisyen, öğrenci taleplerini onaylayabilir/reddedebilir
            bool yetkili = false;
            
            if (User.IsInRole("Ogrenci") && danismanlikGorusmesi.OgrenciId == (await _ogrenciService.GetOgrenciByUserName(User.Identity.Name)).Id && danismanlikGorusmesi.TalepEden == "Akademisyen")
            {
                yetkili = true;
            }
            else if (User.IsInRole("Akademisyen") && danismanlikGorusmesi.AkademisyenId == (await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name)).Id && danismanlikGorusmesi.TalepEden == "Ogrenci")
            {
                yetkili = true;
            }

            if (!yetkili)
            {
                TempData["Error"] = "Bu görüşme talebini onaylama/reddetme yetkiniz bulunmamaktadır.";
                return RedirectToAction(nameof(Index));
            }

            // Durumu güncelle
            string eskiDurum = danismanlikGorusmesi.Durum;
            danismanlikGorusmesi.Durum = durum;
            
            _context.Update(danismanlikGorusmesi);
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await SendStatusChangeNotification(danismanlikGorusmesi, eskiDurum);

            TempData["Message"] = durum == "Onaylandı" 
                ? "Görüşme talebi başarıyla onaylandı." 
                : "Görüşme talebi reddedildi.";

            return RedirectToAction(nameof(Index));
        }

        #region Private Helper Methods

        private async Task<IActionResult> CreateForStudent(int? projeId)
        {
            var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
            
            if (projeId.HasValue)
            {
                var proje = await _context.Projeler
                    .Include(p => p.Mentor)
                    .FirstOrDefaultAsync(p => p.Id == projeId && p.OgrenciId == ogrenci.Id);
                
                if (proje == null)
                {
                    return NotFound();
                }
                
                var gorusme = new DanismanlikGorusmesi
                {
                    ProjeId = projeId.Value,
                    OgrenciId = ogrenci.Id,
                    AkademisyenId = proje.MentorId ?? 0, // Add null-coalescing operator to handle nullable int
                    Baslik = "Danışmanlık Görüşmesi",
                    TalepEden = "Ogrenci",
                    Durum = "Beklemede"
                };
                
                gorusme.GuncelleZamanDurumu();
                
                ViewData["ProjeAd"] = proje.Ad;
                ViewData["AkademisyenAd"] = proje.Mentor?.Ad + " " + proje.Mentor?.Soyad;
                ViewData["UserRole"] = "Ogrenci";
                
                return View(gorusme);
            }
            else
            {
                await PrepareStudentViewData(ogrenci);
                return View();
            }
        }

        private async Task<IActionResult> CreateForAcademician(int? projeId)
        {
            var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
            
            if (projeId.HasValue)
            {
                var proje = await _context.Projeler
                    .Include(p => p.Ogrenci)
                    .FirstOrDefaultAsync(p => p.Id == projeId && p.MentorId == akademisyen.Id);
                
                if (proje == null)
                {
                    return NotFound();
                }
                
                var gorusme = new DanismanlikGorusmesi
                {
                    ProjeId = projeId.Value,
                    OgrenciId = proje.OgrenciId ?? 0, // Add null-coalescing operator to handle nullable int
                    AkademisyenId = akademisyen.Id,
                    Baslik = "Danışmanlık Görüşmesi",
                    TalepEden = "Akademisyen",
                    Durum = "Beklemede"
                };
                
                gorusme.GuncelleZamanDurumu();
                
                ViewData["ProjeAd"] = proje.Ad;
                ViewData["OgrenciAd"] = proje.Ogrenci?.Ad + " " + proje.Ogrenci?.Soyad;
                ViewData["UserRole"] = "Akademisyen";
                
                return View(gorusme);
            }
            else
            {
                await PrepareAcademicianViewData(akademisyen);
                return View();
            }
        }

        private async Task<IActionResult> CreatePostForStudent(DanismanlikGorusmesi danismanlikGorusmesi)
        {
            var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
            danismanlikGorusmesi.OgrenciId = ogrenci.Id;
            danismanlikGorusmesi.Durum = "Beklemede";
            danismanlikGorusmesi.TalepEden = "Ogrenci";
            
            // Zaman durumu güncellemesi
            danismanlikGorusmesi.GuncelleZamanDurumu();
            
            if (ModelState.IsValid)
            {
                _context.Add(danismanlikGorusmesi);
                await _context.SaveChangesAsync();
                
                await _bildirimService.GorusmePlanlandiBildirimiGonder(danismanlikGorusmesi);
                
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareStudentViewData(ogrenci, danismanlikGorusmesi);
            return View(danismanlikGorusmesi);
        }

        private async Task<IActionResult> CreatePostForAcademician(DanismanlikGorusmesi danismanlikGorusmesi)
        {
            var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
            danismanlikGorusmesi.AkademisyenId = akademisyen.Id;
            danismanlikGorusmesi.Durum = "Beklemede";
            danismanlikGorusmesi.TalepEden = "Akademisyen";
            
            // Zaman durumu güncellemesi
            danismanlikGorusmesi.GuncelleZamanDurumu();
            
            if (ModelState.IsValid)
            {
                _context.Add(danismanlikGorusmesi);
                await _context.SaveChangesAsync();
                
                await _bildirimService.GorusmePlanlandiBildirimiGonder(danismanlikGorusmesi);
                
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareAcademicianViewData(akademisyen, danismanlikGorusmesi);
            return View(danismanlikGorusmesi);
        }

        private async Task PrepareStudentViewData(Ogrenci ogrenci, DanismanlikGorusmesi danismanlikGorusmesi = null)
        {
            var projeler = await _context.Projeler
                .Where(p => p.OgrenciId == ogrenci.Id)
                .ToListAsync();
            
            var akademisyenler = await _context.Akademisyenler.ToListAsync();
            
            ViewData["ProjeId"] = new SelectList(projeler, "Id", "Ad", danismanlikGorusmesi?.ProjeId);
            
            ViewData["AkademisyenId"] = new SelectList(akademisyenler.Select(a => new
            {
                Id = a.Id,
                TamAd = $"{a.Unvan} {a.Ad} {a.Soyad}"
            }), "Id", "TamAd", danismanlikGorusmesi?.AkademisyenId);
            
            ViewData["UserRole"] = "Ogrenci";
        }

        private async Task PrepareAcademicianViewData(Akademisyen akademisyen, DanismanlikGorusmesi danismanlikGorusmesi = null)
        {
            var projeler = await _context.Projeler
                .Where(p => p.MentorId == akademisyen.Id)
                .ToListAsync();
            
            var ogrenciIds = projeler.Select(p => p.OgrenciId).Distinct().ToList();
            var ogrenciler = await _context.Ogrenciler
                .Where(o => ogrenciIds.Contains(o.Id))
                .ToListAsync();
            
            ViewData["ProjeId"] = new SelectList(projeler, "Id", "Ad", danismanlikGorusmesi?.ProjeId);
            
            ViewData["OgrenciId"] = new SelectList(ogrenciler.Select(o => new
            {
                Id = o.Id,
                TamAd = $"{o.Ad} {o.Soyad} ({o.OgrenciNo})"
            }), "Id", "TamAd", danismanlikGorusmesi?.OgrenciId);
            
            ViewData["UserRole"] = "Akademisyen";
        }

        private async Task<bool> CanAccessMeeting(DanismanlikGorusmesi meeting)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }
            
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

        private async Task<bool> CanEditMeeting(DanismanlikGorusmesi meeting)
        {
            if (meeting.Durum == "Onaylandı")
            {
                TempData["Error"] = "Onaylanmış görüşmeler düzenlenemez.";
                return false;
            }
            
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);
                if (meeting.OgrenciId != ogrenci.Id)
                {
                    return false;
                }
                
                if (meeting.Durum != "Beklemede")
                {
                    TempData["Error"] = "Sadece beklemede olan görüşmeleri düzenleyebilirsiniz.";
                    return false;
                }
            }
            else if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);
                return meeting.AkademisyenId == akademisyen.Id;
            }
            else if (User.IsInRole("Admin"))
            {
                return true;
            }
            
            return false;
        }

        private async Task PrepareEditViewData(DanismanlikGorusmesi meeting)
        {
            var projeler = await _context.Projeler.ToListAsync();
            ViewData["ProjeId"] = new SelectList(projeler, "Id", "Ad", meeting.ProjeId);
        }

        private async Task UpdateMeeting(DanismanlikGorusmesi danismanlikGorusmesi, DanismanlikGorusmesi mevcutGorusme)
        {
            // Her zaman önemli ilişki kimliklerini koru
            danismanlikGorusmesi.OgrenciId = mevcutGorusme.OgrenciId;
            danismanlikGorusmesi.AkademisyenId = mevcutGorusme.AkademisyenId;
            danismanlikGorusmesi.TalepEden = mevcutGorusme.TalepEden;
            
            // Preserve certain fields based on user role
            if (User.IsInRole("Ogrenci"))
            {
                danismanlikGorusmesi.Durum = mevcutGorusme.Durum;
            }
            
            // Zaman durumu güncellemesi
            danismanlikGorusmesi.GuncelleZamanDurumu();
            
            _context.Update(danismanlikGorusmesi);
            await _context.SaveChangesAsync();
            
            // Send notification if status changed
            if (mevcutGorusme.Durum != danismanlikGorusmesi.Durum)
            {
                await SendStatusChangeNotification(danismanlikGorusmesi, mevcutGorusme.Durum);
            }
        }

        private async Task SendStatusChangeNotification(DanismanlikGorusmesi meeting, string oldStatus)
        {
            string bildirimBaslik;
            string bildirimIcerik;
            
            switch (meeting.Durum)
            {
                case "Onaylandı":
                    bildirimBaslik = "Görüşme Talebi Onaylandı";
                    bildirimIcerik = $"Görüşme talebiniz akademisyen tarafından onaylandı. Tarih: {meeting.GorusmeTarihi:g}";
                    break;
                case "Reddedildi":
                    bildirimBaslik = "Görüşme Talebi Reddedildi";
                    bildirimIcerik = "Görüşme talebiniz akademisyen tarafından reddedildi.";
                    break;
                default:
                    bildirimBaslik = "Görüşme Talebi Güncellendi";
                    bildirimIcerik = "Görüşme talebinizde değişiklik yapıldı.";
                    break;
            }
            
            var bildirim = new Bildirim
            {
                Baslik = bildirimBaslik,
                Icerik = bildirimIcerik,
                OgrenciId = meeting.OgrenciId,
                BildirimTipi = "Bilgi"
            };
            
            _context.Bildirimler.Add(bildirim);
            await _context.SaveChangesAsync();
        }

        private bool DanismanlikGorusmesiExists(int id)
        {
            return _context.DanismanlikGorusmeleri.Any(e => e.Id == id);
        }

        #endregion
    }
}
