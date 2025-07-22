using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Admin,Akademisyen")]
    public class ProjeAtamaController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly IOgrenciService _ogrenciService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly AuthService _authService;

        public ProjeAtamaController(
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            AuthService authService)
        {
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
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

        // GET: ProjeAtama/Assign/5
        public async Task<IActionResult> Assign(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            // Proje durumu "Beklemede" değilse, sadece admin veya danışman değişiklik yapabilir
            if (proje.Status != "Beklemede" && !await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projede değişiklik yapmak için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id });
            }

            var ogrenciler = await _ogrenciService.GetAllAsync();
            var akademisyenler = await _akademisyenService.GetAllAsync();

            // Mevcut kullanıcı projenin danışmanı mı kontrol et
            ViewBag.IsCurrentUserMentor = await IsCurrentUserProjectMentor(id);

            // Eğer akademisyen ise, kendi ID'sini ViewBag'e ekle
            if (User.IsInRole("Akademisyen"))
            {
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                if (akademisyen != null)
                {
                    ViewBag.CurrentAkademisyenId = akademisyen.Id;
                    ViewBag.CurrentAkademisyenAd = $"{akademisyen.Unvan} {akademisyen.Ad} {akademisyen.Soyad}";
                }
            }

            ViewBag.Ogrenciler = new SelectList(ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.Ad} {o.Soyad}" }), "Id", "AdSoyad");
            ViewBag.Akademisyenler = new SelectList(akademisyenler.Select(a => new { Id = a.Id, AdSoyad = $"{a.Unvan} {a.Ad} {a.Soyad}" }), "Id", "AdSoyad");
            
            // Proje klasöründeki Assign view'ini kullanalım
            return View("~/Views/Proje/Assign.cshtml", proje);
        }

        // POST: ProjeAtama/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int ogrenciId, int mentorId)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            // Proje durumu "Beklemede" değilse, sadece admin veya danışman değişiklik yapabilir
            bool isProjectMentor = await IsCurrentUserProjectMentor(id);
            if (proje.Status != "Beklemede" && !isProjectMentor)
            {
                TempData["ErrorMessage"] = "Bu projede değişiklik yapmak için yetkiniz yok.";
                return RedirectToAction("Details", "Proje", new { id });
            }

            // Akademisyen kendisini danışman olarak atıyorsa veya admin/mevcut danışman herhangi birini atıyorsa
            var akademisyen = await _authService.GetCurrentAkademisyenAsync();
            if (akademisyen != null && !User.IsInRole("Admin") && !isProjectMentor)
            {
                // Akademisyen sadece kendisini danışman olarak atayabilir
                if (akademisyen.Id != mentorId)
                {
                    TempData["ErrorMessage"] = "Sadece kendinizi danışman olarak atayabilirsiniz.";
                    return RedirectToAction("Details", "Proje", new { id });
                }
            }
            
            // Öğrenciyi sadece hiç atanmamışsa ata (yeni proje durumunda)
            if (proje.OgrenciId == null && ogrenciId > 0)
            {
                await _projeService.AssignToOgrenciAsync(id, ogrenciId);
            }
            
            // Danışman değişikliği
            if (mentorId > 0)
            {
                // Önceki durumu sakla
                var oncekiDurum = proje.Status;
                
                await _projeService.AssignToMentorAsync(id, mentorId);
                
                // Proje durumunu "Atanmis" olarak güncelle - eğer önceki durum "Beklemede" ise
                if (oncekiDurum == "Beklemede")
                {
                    await _projeService.UpdateStatusAsync(id, "Atanmis");
                    TempData["SuccessMessage"] = "Proje danışmanı başarıyla atandı ve proje durumu 'Atanmış' olarak güncellendi.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Proje danışmanı başarıyla atandı.";
                }
            }
            
            return RedirectToAction("Details", "Proje", new { id });
        }
    }
} 