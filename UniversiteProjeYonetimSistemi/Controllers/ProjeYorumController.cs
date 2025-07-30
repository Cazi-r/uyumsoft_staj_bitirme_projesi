using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeYorumController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;
        private readonly IBildirimService _bildirimService;

        public ProjeYorumController(
            IProjeService projeService,
            AuthService authService,
            IBildirimService bildirimService)
        {
            _projeService = projeService;
            _authService = authService;
            _bildirimService = bildirimService;
        }

        // POST: ProjeYorum/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProjeYorumViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen yorum alanını doldurun.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            int? ogrenciId = null;
            int? akademisyenId = null;

            // Yorum yapan kişiyi belirle
            if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                if (akademisyen != null)
                {
                    akademisyenId = akademisyen.Id;
                }
            }
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                if (ogrenci != null)
                {
                    ogrenciId = ogrenci.Id;
                }
            }
            // Admin rolü için akademisyen ID 29 kullan
            else if (User.IsInRole("Admin"))
            {
                akademisyenId = 19; // ID 29 olan akademisyen üzerinden yorum ekle
            }

            var yorum = await _projeService.AddCommentAsync(model.ProjeId, model.Icerik, model.YorumTipi, ogrenciId, akademisyenId);
            
            // Yorum yapıldığına dair bildirim gönder
            if (yorum != null)
            {
                await _bildirimService.ProjeYorumYapildiBildirimiGonder(yorum, model.ProjeId);
            }
            
            TempData["SuccessMessage"] = "Yorum başarıyla eklendi.";

            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // GET: ProjeYorum/Index
        public async Task<IActionResult> Index(int projeId)
        {
            // Proje var mı kontrol et
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null)
            {
                TempData["ErrorMessage"] = "Proje bulunamadı.";
                return RedirectToAction("Index", "Proje");
            }

            // Kullanıcının bu projeye erişim yetkisi var mı kontrol et
            bool hasPermission = false;
            if (User.IsInRole("Admin"))
            {
                hasPermission = true;
            }
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                hasPermission = ogrenci != null && proje.OgrenciId == ogrenci.Id;
            }
            else if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                hasPermission = akademisyen != null && proje.MentorId == akademisyen.Id;
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu projeye erişim yetkiniz bulunmuyor.";
                return RedirectToAction("Index", "Proje");
            }

            var yorumlar = await _projeService.GetCommentsByProjeIdAsync(projeId);
            return View(yorumlar);
        }



        // POST: ProjeYorum/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            var yorumlar = await _projeService.GetCommentsByProjeIdAsync(projeId);
            var yorum = yorumlar.FirstOrDefault(y => y.Id == id);
                
            if (yorum == null)
            {
                return NotFound();
            }
            
            bool canDelete = false;
            
            // Admin her zaman silebilir
            if (User.IsInRole("Admin"))
            {
                canDelete = true;
            }
            // Akademisyen kendi yorumunu silebilir
            else if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                canDelete = akademisyen != null && yorum.AkademisyenId == akademisyen.Id;
            }
            // Öğrenci kendi yorumunu silebilir
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                canDelete = ogrenci != null && yorum.OgrenciId == ogrenci.Id;
            }
            
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Bu yorumu silme yetkiniz yok.";
                return RedirectToAction("Index", "ProjeYorum", new { projeId = projeId });
            }
            
            await _projeService.DeleteCommentAsync(id);
            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            return RedirectToAction("Index", "ProjeYorum", new { projeId = projeId });
        }
    }
} 