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
        private readonly IBildirimService _bildirimService;

        public ProjeDegerlendirmeController(
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
            
            ViewBag.Proje = proje;
            
            return View(proje);
        }

        // POST: ProjeDegerlendirme/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Add(int projeId, int puan, string aciklama, string degerlendirmeTipi)
        {
            // DegerlendirmeOlustur view'i ViewModel yerine ayrı parametreler bekliyor
            
            if (string.IsNullOrEmpty(degerlendirmeTipi) || string.IsNullOrEmpty(aciklama) || puan < 0 || puan > 100)
            {
                var proje = await _projeService.GetByIdAsync(projeId);
                if (proje == null)
                {
                    return NotFound();
                }
                
                ViewBag.Proje = proje;
                TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun ve puanı 0-100 arasında belirtin.";
                return View(proje);
            }

            var akademisyen = await _authService.GetCurrentAkademisyenAsync();
            if (akademisyen == null)
            {
                TempData["ErrorMessage"] = "Akademisyen bilgilerinize erişilemedi.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            var degerlendirme = await _projeService.AddEvaluationAsync(projeId, puan, aciklama, degerlendirmeTipi, akademisyen.Id);
            
            // Değerlendirme yapıldığına dair bildirim gönder
            if (degerlendirme != null)
            {
                await _bildirimService.DegerlendirmeYapildiBildirimiGonder(degerlendirme);
            }
            
            TempData["SuccessMessage"] = "Değerlendirme başarıyla eklendi.";

            return RedirectToAction("Details", "Proje", new { id = projeId });
        }

        // GET: ProjeDegerlendirme/Index
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
                hasPermission = await IsCurrentUserProjectMentor(projeId);
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu projeye erişim yetkiniz bulunmuyor.";
                return RedirectToAction("Index", "Proje");
            }

            var degerlendirmeler = await _projeService.GetEvaluationsByProjeIdAsync(projeId);
            return View(degerlendirmeler);
        }

        // GET: ProjeDegerlendirme/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var degerlendirme = await _projeService.GetEvaluationByIdAsync(id);
            
            if (degerlendirme == null)
            {
                TempData["ErrorMessage"] = "Değerlendirme bulunamadı.";
                return NotFound();
            }

            // Kullanıcının bu değerlendirmeye erişim yetkisi var mı kontrol et
            bool hasPermission = false;
            if (User.IsInRole("Admin"))
            {
                hasPermission = true;
            }
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                hasPermission = ogrenci != null && degerlendirme.Proje.OgrenciId == ogrenci.Id;
            }
            else if (User.IsInRole("Akademisyen"))
            {
                hasPermission = await IsCurrentUserProjectMentor(degerlendirme.ProjeId);
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu değerlendirmeye erişim yetkiniz bulunmuyor.";
                return RedirectToAction("Index", "Proje");
            }

            return View(degerlendirme);
        }

        // GET: ProjeDegerlendirme/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var degerlendirme = await _projeService.GetEvaluationByIdAsync(id);
            
            if (degerlendirme == null)
            {
                TempData["ErrorMessage"] = "Değerlendirme bulunamadı.";
                return NotFound();
            }

            // Sadece projenin danışmanı değerlendirme düzenleyebilir
            if (!await IsCurrentUserProjectMentor(degerlendirme.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu değerlendirmeyi sadece projenin danışmanı veya admin düzenleyebilir.";
                return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = degerlendirme.ProjeId });
            }

            return View(degerlendirme);
        }

        // POST: ProjeDegerlendirme/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id, int projeId, int puan, string aciklama, string degerlendirmeTipi)
        {
            if (string.IsNullOrEmpty(degerlendirmeTipi) || string.IsNullOrEmpty(aciklama) || puan < 0 || puan > 100)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun ve puanı 0-100 arasında belirtin.";
                return RedirectToAction("Edit", new { id = id });
            }

            // Sadece projenin danışmanı değerlendirme düzenleyebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu değerlendirmeyi sadece projenin danışmanı veya admin düzenleyebilir.";
                return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = projeId });
            }

            await _projeService.UpdateEvaluationAsync(id, puan, aciklama, degerlendirmeTipi);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla güncellendi.";

            return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = projeId });
        }

        // GET: ProjeDegerlendirme/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var degerlendirme = await _projeService.GetEvaluationByIdAsync(id);
            
            if (degerlendirme == null)
            {
                TempData["ErrorMessage"] = "Değerlendirme bulunamadı.";
                return NotFound();
            }

            // Sadece projenin danışmanı değerlendirme silebilir
            if (!await IsCurrentUserProjectMentor(degerlendirme.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu değerlendirmeyi silebilir.";
                return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = degerlendirme.ProjeId });
            }

            return View(degerlendirme);
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
                return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = projeId });
            }
            
            await _projeService.DeleteEvaluationAsync(id);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla silindi.";
            
            return RedirectToAction("Index", "ProjeDegerlendirme", new { projeId = projeId });
        }
    }
} 