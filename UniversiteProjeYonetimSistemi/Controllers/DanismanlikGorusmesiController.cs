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
        private readonly ILogger<DanismanlikGorusmesiController> _logger;

        public DanismanlikGorusmesiController(
            ApplicationDbContext context, 
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IBildirimService bildirimService,
            ILogger<DanismanlikGorusmesiController> logger)
        {
            _context = context;
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _bildirimService = bildirimService;
            _logger = logger;
        }

        // Index: Rol'e gore (Ogrenci/Akademisyen/Admin) farkli gorunum ve veri setleri dondurur.
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
                    .Where(g => g.Durum == GorusmeDurumu.HocaOnayiBekliyor && g.TalepEden == "Ogrenci")
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                
                ViewBag.YakinGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == GorusmeDurumu.Onaylandi && (g.ZamanDurumu == "Bugun" || g.ZamanDurumu == "YakinGelecek"))
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                    
                ViewBag.IleridekiGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == GorusmeDurumu.Onaylandi && g.ZamanDurumu == "UzakGelecek")
                    .OrderBy(g => g.GorusmeTarihi)
                    .ToList();
                    
                ViewBag.GecmisGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == GorusmeDurumu.Onaylandi && g.ZamanDurumu == "Gecmis")
                    .OrderByDescending(g => g.GorusmeTarihi)
                    .ToList();
                
                ViewBag.IptalGorusmeler = akademisyenGorusmeleri
                    .Where(g => g.Durum == GorusmeDurumu.IptalEdildi)
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

        // Create GET: Ogrenci/Akademisyen icin farkli form akislari; [Authorize(Roles = "Ogrenci,Akademisyen")] ile korunur.
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

        // Create POST: Ogrenci/Akademisyen icin ayri post akislari calistirilir, bildirim gonderilir.
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

        // Edit GET: Yetki ve is kurali kontrolleri (onaylanmis gorusme duzenlenemez vb.) sonra formu dondurur.
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

        // Edit POST: rol'e gore yeni durumu ayarlar, zaman durumunu ve bildirimleri gunceller.
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

        // Onay/Reddet: Hem Ogrenci hem Akademisyen icin is kurallarina gore karar uygular.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Akademisyen,Ogrenci")]
        public async Task<IActionResult> OnayveyaReddet(int id, string karar)
        {
            return await GorusmeYanitla(id, karar);
        }

        // Onay/Reddet islemini yapan ortak metod; yetki kontrolu ve bildirim gonderimi icerir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Akademisyen,Ogrenci")]
        public async Task<IActionResult> GorusmeYanitla(int id, string karar)
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

            GorusmeDurumu yeniDurum;
            if (karar == "Onayla")
            {
                yeniDurum = GorusmeDurumu.Onaylandi;
            }
            else if (karar == "Reddet")
            {
                yeniDurum = GorusmeDurumu.IptalEdildi;
            }
            else
            {
                TempData["Error"] = "Geçersiz işlem.";
                return RedirectToAction(nameof(Index));
            }

            var guncelleyenRol = User.IsInRole("Ogrenci") ? "Ogrenci" : "Akademisyen";

            // İş mantığına göre kimin onaylaması gerektiğini kontrol et
            bool islemGecerli = (guncelleyenRol == "Ogrenci" && danismanlikGorusmesi.Durum == GorusmeDurumu.OgrenciOnayiBekliyor && danismanlikGorusmesi.SonGuncelleyenRol == "Akademisyen") ||
                               (guncelleyenRol == "Akademisyen" && danismanlikGorusmesi.Durum == GorusmeDurumu.HocaOnayiBekliyor && danismanlikGorusmesi.SonGuncelleyenRol == "Ogrenci");

            _logger.LogInformation("Onay/Reddet İşlemi Denetimi: \n ID: {Id}, \n Karar: {Karar}, \n Mevcut Durum: {MevcutDurum}, \n İşlemi Yapan Rol: {GuncelleyenRol}, \n Son Güncelleyen Rol: {SonGuncelleyenRol}, \n İşlem Geçerli mi: {IslemGecerli}", 
                id, karar, danismanlikGorusmesi.Durum, guncelleyenRol, danismanlikGorusmesi.SonGuncelleyenRol, islemGecerli);

            if (!islemGecerli)
            {
                TempData["Error"] = "Bu işlemi şu anda gerçekleştiremezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            // Durumu güncelle
            var eskiDurum = danismanlikGorusmesi.Durum;
            danismanlikGorusmesi.Durum = yeniDurum;
            danismanlikGorusmesi.SonGuncelleyenRol = guncelleyenRol;
            
            _context.Update(danismanlikGorusmesi);
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _bildirimService.GorusmeDurumuDegistiBildirimiGonder(danismanlikGorusmesi);

            TempData["Message"] = yeniDurum == GorusmeDurumu.Onaylandi 
                ? "Görüşme talebi başarıyla onaylandı." 
                : "Görüşme talebi iptal edildi.";

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
                    Durum = GorusmeDurumu.HocaOnayiBekliyor, // İlk talep hocanın onayına gider
                    SonGuncelleyenRol = "Ogrenci"
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
                    Durum = GorusmeDurumu.OgrenciOnayiBekliyor, // Hoca talep edince öğrenci onayına gider
                    SonGuncelleyenRol = "Akademisyen"
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
            danismanlikGorusmesi.Durum = GorusmeDurumu.HocaOnayiBekliyor;
            danismanlikGorusmesi.TalepEden = "Ogrenci";
            danismanlikGorusmesi.SonGuncelleyenRol = "Ogrenci";
            
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
            danismanlikGorusmesi.Durum = GorusmeDurumu.OgrenciOnayiBekliyor;
            danismanlikGorusmesi.TalepEden = "Akademisyen";
            danismanlikGorusmesi.SonGuncelleyenRol = "Akademisyen";
            
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
            if (meeting.Durum == GorusmeDurumu.Onaylandi)
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
                
                if (meeting.Durum != GorusmeDurumu.HocaOnayiBekliyor && meeting.Durum != GorusmeDurumu.OgrenciOnayiBekliyor)
                {
                    TempData["Error"] = "Sadece onay bekleyen görüşmeleri düzenleyebilirsiniz.";
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
            string guncelleyenRol = "";
            if (User.IsInRole("Ogrenci"))
            {
                danismanlikGorusmesi.Durum = GorusmeDurumu.HocaOnayiBekliyor;
                guncelleyenRol = "Ogrenci";
            }
            else if (User.IsInRole("Akademisyen"))
            {
                danismanlikGorusmesi.Durum = GorusmeDurumu.OgrenciOnayiBekliyor;
                guncelleyenRol = "Akademisyen";
            }
            danismanlikGorusmesi.SonGuncelleyenRol = guncelleyenRol;
            
            // Zaman durumu güncellemesi
            danismanlikGorusmesi.GuncelleZamanDurumu();
            
            _context.Update(danismanlikGorusmesi);
            await _context.SaveChangesAsync();
            
            // Send notification if status changed
            if (mevcutGorusme.Durum != danismanlikGorusmesi.Durum)
            {
                await _bildirimService.GorusmeDurumuDegistiBildirimiGonder(danismanlikGorusmesi);
            }
        }

        private bool DanismanlikGorusmesiExists(int id)
        {
            return _context.DanismanlikGorusmeleri.Any(e => e.Id == id);
        }
        #endregion
    }
}
