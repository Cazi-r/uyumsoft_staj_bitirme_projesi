using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeDegerlendirmeController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;

        public ProjeDegerlendirmeController(
            IProjeService projeService,
            AuthService authService)
        {
            _projeService = projeService;
            _authService = authService;
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

        // GET: ProjeDegerlendirme/Add/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı değerlendirme ekleyebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeye sadece danışmanı veya admin değerlendirme ekleyebilir.";
                return RedirectToAction("Details", "Proje", new { id });
            }
            
            var model = new DegerlendirmeViewModel
            {
                ProjeId = id
            };
            
            ViewBag.DegerlendirmeTipleri = new SelectList(new[] { "Ara", "Final", "Genel" });
            return View(model);
        }

        // POST: ProjeDegerlendirme/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(DegerlendirmeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DegerlendirmeTipleri = new SelectList(new[] { "Ara", "Final", "Genel" });
                return View(model);
            }

            var akademisyen = await _authService.GetCurrentAkademisyenAsync();
            if (akademisyen == null)
            {
                TempData["ErrorMessage"] = "Akademisyen bilgilerinize erişilemedi.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            await _projeService.AddEvaluationAsync(model.ProjeId, model.Puan, model.Aciklama, model.DegerlendirmeTipi, akademisyen.Id);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla eklendi.";

            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // POST: ProjeDegerlendirme/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            var degerlendirmeler = await _projeService.GetEvaluationsByProjeIdAsync(projeId);
            var evaluation = degerlendirmeler.FirstOrDefault(d => d.Id == id);
                
            if (evaluation == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı değerlendirme silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu değerlendirmeyi sadece projenin danışmanı veya admin silebilir.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }
            
            await _projeService.DeleteEvaluationAsync(id);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla silindi.";
            
            return RedirectToAction("Details", "Proje", new { id = projeId });
        }
    }
} 