using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using UniversiteProjeYonetimSistemi.Data;
using System.Linq;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeAsamaController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;
        private readonly IBildirimService _bildirimService;
        private readonly ApplicationDbContext _context;

        public ProjeAsamaController(
            IProjeService projeService,
            AuthService authService,
            IBildirimService bildirimService,
            ApplicationDbContext context)
        {
            _projeService = projeService;
            _authService = authService;
            _bildirimService = bildirimService;
            _context = context;
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

            return View(model);
        }

        // POST: ProjeAsama/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(ProjeAsamasi model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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

        // GET: ProjeAsama/Index/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> Index(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            var asamalar = await _projeService.GetStagesByProjeIdAsync(id);
            return View(asamalar);
        }

        // GET: ProjeAsama/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id)
        {
            // Tüm projelerin aşamalarını al ve belirtilen ID'ye sahip aşamayı bul
            var allAsamalar = await _context.ProjeAsamalari
                .Include(a => a.Proje)
                .ToListAsync();
            var asama = allAsamalar.FirstOrDefault(a => a.Id == id);
            
            if (asama == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı aşama düzenleyebilir
            if (!await IsCurrentUserProjectMentor(asama.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu aşamayı sadece projenin danışmanı veya admin düzenleyebilir.";
                return RedirectToAction("Details", "Proje", new { id = asama.ProjeId });
            }
            
            return View(asama);
        }

        // POST: ProjeAsama/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(ProjeAsamasi model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Sadece projenin danışmanı aşama düzenleyebilir
            if (!await IsCurrentUserProjectMentor(model.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu aşamayı sadece projenin danışmanı veya admin düzenleyebilir.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }
            
            // Aciklama null ise boş string olarak ayarla
            string aciklama = model.Aciklama ?? "";
            
            await _projeService.UpdateStageAsync(
                model.Id,
                model.AsamaAdi,
                aciklama,
                model.BaslangicTarihi,
                model.BitisTarihi,
                model.SiraNo,
                model.Tamamlandi);
                
            TempData["SuccessMessage"] = "Proje aşaması başarıyla güncellendi.";
            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // GET: ProjeAsama/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Delete(int id)
        {
            // Tüm projelerin aşamalarını al ve belirtilen ID'ye sahip aşamayı bul
            var allAsamalar = await _context.ProjeAsamalari
                .Include(a => a.Proje)
                .ToListAsync();
            var asama = allAsamalar.FirstOrDefault(a => a.Id == id);
            
            if (asama == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı aşama silebilir
            if (!await IsCurrentUserProjectMentor(asama.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu aşamayı sadece projenin danışmanı veya admin silebilir.";
                return RedirectToAction("Details", "Proje", new { id = asama.ProjeId });
            }
            
            return View(asama);
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