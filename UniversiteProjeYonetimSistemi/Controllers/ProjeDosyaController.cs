using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeDosyaController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly AuthService _authService;

        public ProjeDosyaController(
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

        // POST: ProjeDosya/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ProjeDosyaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            if (model.Dosya != null && model.Dosya.Length > 0)
            {
                int? yukleyenId = null;
                string yukleyenTipi = "Ogrenci";

                // Yükleyen bilgisini belirle
                if (User.IsInRole("Akademisyen"))
                {
                    var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                    if (akademisyen != null)
                    {
                        yukleyenId = akademisyen.Id;
                        yukleyenTipi = "Akademisyen";
                    }
                }
                else if (User.IsInRole("Ogrenci"))
                {
                    var ogrenci = await _authService.GetCurrentOgrenciAsync();
                    if (ogrenci != null)
                    {
                        yukleyenId = ogrenci.Id;
                        yukleyenTipi = "Ogrenci";
                    }
                }

                await _projeService.UploadFileAsync(model.ProjeId, model.Dosya, model.Aciklama, yukleyenId, yukleyenTipi);
                TempData["SuccessMessage"] = "Dosya başarıyla yüklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Geçersiz dosya.";
            }

            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // GET: ProjeDosya/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var dosya = await _projeService.GetFileByIdAsync(id);
            if (dosya == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dosya.DosyaYolu.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", dosya.DosyaAdi);
        }

        // POST: ProjeDosya/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            // Sadece projenin danışmanı dosya silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu projeye ait dosyaları sadece danışmanı veya admin silebilir.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            await _projeService.DeleteFileAsync(id);
            TempData["SuccessMessage"] = "Dosya başarıyla silindi.";
            return RedirectToAction("Details", "Proje", new { id = projeId });
        }
    }
} 