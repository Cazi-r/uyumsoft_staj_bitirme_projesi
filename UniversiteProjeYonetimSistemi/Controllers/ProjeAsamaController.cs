using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using System.Linq;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeAsamaController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;
        private readonly IBildirimService _bildirimService;

        public ProjeAsamaController(
            IProjeService projeService,
            AuthService authService,
            IBildirimService bildirimService)
        {
            _projeService = projeService;
            _authService = authService;
            _bildirimService = bildirimService;
        }

        // Helper method to check if current user is the mentor of the project
        private async Task<bool> IsCurrentUserProjectMentor(int projeId)
        {
            // Admin her zaman tüm yetkilere sahiptir
            if (User.IsInRole("Admin"))
            {
                return true;
            }
            
            // Kullanıcı akademisyen değilse yetkisi yok
            if (!User.IsInRole("Akademisyen"))
            {
                return false;
            }
            
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null || !proje.MentorId.HasValue)
            {
                return false;
            }
            
            var akademisyen = await _authService.GetCurrentAkademisyenAsync();
            if (akademisyen == null)
            {
                return false;
            }
            
            // Projenin danışmanı mı kontrol et
            return proje.MentorId.Value == akademisyen.Id;
        }

        // GET: ProjeAsama/Add/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı aşama ekleyebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeye sadece danışmanı veya admin aşama ekleyebilir.";
                return RedirectToAction("Details", "Proje", new { id });
            }

            var model = new ProjeAsamasi
            {
                ProjeId = id,
                SiraNo = (await _projeService.GetStagesByProjeIdAsync(id)).Count() + 1
            };

            // Proje klasöründeki AddStage view'ini kullanalım
            return View("~/Views/Proje/AddStage.cshtml", model);
        }

        // POST: ProjeAsama/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(ProjeAsamasi model)
        {
            if (!ModelState.IsValid)
            {
                // Post işleminde de aynı view'i kullanalım
                return View("~/Views/Proje/AddStage.cshtml", model);
            }

            // Aciklama null ise boş string olarak ayarla
            string aciklama = model.Aciklama ?? "";

            await _projeService.AddStageAsync(
                model.ProjeId, 
                model.AsamaAdi, 
                aciklama, 
                model.BaslangicTarihi, 
                model.BitisTarihi, 
                model.SiraNo);
                
            TempData["SuccessMessage"] = "Proje aşaması başarıyla eklendi.";

            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // POST: ProjeAsama/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> UpdateStatus(int id, int projeId, bool tamamlandi)
        {
            await _projeService.UpdateStageStatusAsync(id, tamamlandi);
            
            // Eğer aşama tamamlandı olarak işaretlendiyse ve öğrenci veya admin tarafından yapıldıysa bildirim gönder
            if (tamamlandi && (User.IsInRole("Ogrenci") || User.IsInRole("Admin")))
            {
                // Aşamayı getir
                var asamalar = await _projeService.GetStagesByProjeIdAsync(projeId);
                var asama = asamalar.FirstOrDefault(a => a.Id == id);
                
                if (asama != null)
                {
                    // Bildirim gönder
                    await _bildirimService.ProjeAsamasiTamamlandiBildirimiGonder(asama);
                }
            }
            
            TempData["SuccessMessage"] = tamamlandi ? "Aşama tamamlandı olarak işaretlendi." : "Aşama devam ediyor olarak işaretlendi.";
            return RedirectToAction("Details", "Proje", new { id = projeId });
        }

        // POST: ProjeAsama/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            // Sadece projenin danışmanı aşama silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu projenin aşamalarını sadece danışmanı veya admin silebilir.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }
            
            await _projeService.DeleteStageAsync(id);
            TempData["SuccessMessage"] = "Proje aşaması başarıyla silindi.";
            return RedirectToAction("Details", "Proje", new { id = projeId });
        }
    }
} 