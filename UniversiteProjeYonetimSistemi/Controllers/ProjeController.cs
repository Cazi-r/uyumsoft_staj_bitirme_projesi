using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize]
    public class ProjeController : Controller
    {
        private readonly IProjeService _projeService;
        private readonly IOgrenciService _ogrenciService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IRepository<ProjeKategori> _kategoriRepository;
        private readonly AuthService _authService;
        private readonly IBildirimService _bildirimService;

        public ProjeController(
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IRepository<ProjeKategori> kategoriRepository,
            AuthService authService,
            IBildirimService bildirimService)
        {
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _kategoriRepository = kategoriRepository;
            _authService = authService;
            _bildirimService = bildirimService;
        }

        // Tum projeleri listeler. [Authorize] ile sadece girisli kullanicilar erisebilir.
        public async Task<IActionResult> Index()
        {
            var projeler = await _projeService.GetAllAsync();
            return View(projeler);
        }

        // Tek proje detayini gosterir; ViewBag ile danismanlik ve yorum bolumu isaretleri doldurulur.
        public async Task<IActionResult> Details(int id, bool? highlightYorum = false)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            // Şu anki kullanıcının projenin danışmanı olup olmadığını kontrol et
            ViewBag.IsCurrentUserMentor = await IsCurrentUserProjectMentor(id);
            
            // Yorum bölümünü highlight etmek için
            ViewBag.HighlightYorum = highlightYorum ?? false;

            return View(proje);
        }

        // Proje olusturma formu; Admin/Akademisyen/Ogrenci erisebilir. Ogrenci icin otomatik alan doldurma yapilir.
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownDataAsync();
            
            // Öğrenciler için otomatik doldurulacak alan bilgileri
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                if (ogrenci != null)
                {
                    ViewBag.OgrenciId = ogrenci.Id;
                    ViewBag.OgrenciAd = $"{ogrenci.Ad} {ogrenci.Soyad}";
                }
            }
            
            return View();
        }

        // Proje olusturma POST: olusturan role gore bildirim gonderilir, durum 'Beklemede' atanir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> Create(Proje proje, bool KaynakEklemekIstiyorum = false)
        {
            if (ModelState.IsValid)
            {
                // Öğrenci kendisi proje oluşturuyorsa OgrenciId'yi otomatik doldur
                if (User.IsInRole("Ogrenci") && proje.OgrenciId == 0)
                {
                    var ogrenci = await _authService.GetCurrentOgrenciAsync();
                    if (ogrenci != null)
                    {
                        proje.OgrenciId = ogrenci.Id;
                    }
                }
                
                // Her zaman yeni projelerin durumunu "Beklemede" olarak ayarla
                proje.Status = "Beklemede";
                
                await _projeService.AddAsync(proje);

                // Projenin oluşturulduğuna dair bildirim gönder
                string olusturanRol = User.IsInRole("Akademisyen") ? "Akademisyen" : "Ogrenci";
                await _bildirimService.ProjeOlusturulduBildirimiGonder(proje, olusturanRol);
                
                // Kaynak eklemek isteniyorsa, detay sayfasına yönlendir ve modal'ı aç
                if (KaynakEklemekIstiyorum)
                {
                    TempData["OpenResourceModal"] = true;
                    return RedirectToAction(nameof(Details), new { id = proje.Id });
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            await LoadDropdownDataAsync();
            
            // Öğrenciler için otomatik doldurulacak alan bilgileri (validation hatası olduğunda)
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                if (ogrenci != null)
                {
                    ViewBag.OgrenciId = ogrenci.Id;
                    ViewBag.OgrenciAd = $"{ogrenci.Ad} {ogrenci.Soyad}";
                }
            }
            
            return View(proje);
        }

        // Proje duzenleme formu: Yalnizca admin veya projenin danismani erisebilir.
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı düzenleyebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeyi sadece danışmanı veya admin düzenleyebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            await LoadDropdownDataAsync();
            return View(proje);
        }

        // Proje duzenleme POST: yalnizca projenin danismani duzenleyebilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id, Proje proje)
        {
            if (id != proje.Id)
            {
                return NotFound();
            }

            // Sadece projenin danışmanı düzenleyebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeyi sadece danışmanı veya admin düzenleyebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _projeService.UpdateAsync(proje);
                    TempData["SuccessMessage"] = "Proje başarıyla güncellendi.";
                    return RedirectToAction(nameof(Details), new { id = proje.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjeExists(proje.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            await LoadDropdownDataAsync();
            return View(proje);
        }

        // Proje silme onayi: Yalnizca admin erisebilir.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            return View(proje);
        }

        // Proje silme POST: yalnizca admin silebilir.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _projeService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Proje başarıyla silindi.";
            return RedirectToAction(nameof(Index));
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

        // Durumu 'Atanmis' -> 'Devam' yapar; yalnizca admin veya projenin danismani ve CSRF korumali.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> UpdateStatusToInProgress(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı durumu değiştirebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeyi sadece danışmanı veya admin güncelleyebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Sadece "Atanmis" durumundaki projeleri "Devam" durumuna geçirebilir
            if (proje.Status == "Atanmis")
            {
                await _projeService.UpdateStatusAsync(id, "Devam");
                
                // Proje durumu değiştiği için bildirim gönder
                proje.Status = "Devam"; // Bildirim servisi için durumu güncelle
                await _bildirimService.ProjeIlerlemesiDegistiBildirimiGonder(proje);
                
                TempData["SuccessMessage"] = "Proje durumu 'Devam Ediyor' olarak güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Proje durumu güncellenemedi. Proje durumu 'Atanmış' olmalıdır.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
        
        // Durumu 'Devam' -> 'Tamamlandi' yapar; yalnizca admin veya projenin danismani ve CSRF korumali.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> CompleteProject(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı durumu değiştirebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeyi sadece danışmanı veya admin tamamlandı olarak işaretleyebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Sadece "Devam" durumundaki projeleri "Tamamlandi" durumuna geçirebilir
            if (proje.Status == "Devam")
            {
                await _projeService.UpdateStatusAsync(id, "Tamamlandi");
                
                // Proje durumu değiştiği için bildirim gönder
                proje.Status = "Tamamlandi"; // Bildirim servisi için durumu güncelle
                await _bildirimService.ProjeIlerlemesiDegistiBildirimiGonder(proje);
                
                TempData["SuccessMessage"] = "Proje başarıyla tamamlandı olarak işaretlendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Proje tamamlandı olarak işaretlenemedi. Proje durumu 'Devam Ediyor' olmalıdır.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
        
        // Projeyi iptal eder; 'Tamamlandi' ise iptal edilemez. Admin/danisman yetkisi ve CSRF korumasi vardir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> CancelProject(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Sadece projenin danışmanı durumu değiştirebilir
            if (!await IsCurrentUserProjectMentor(id))
            {
                TempData["ErrorMessage"] = "Bu projeyi sadece danışmanı veya admin iptal edebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Tamamlanmış projeler iptal edilemez
            if (proje.Status == "Tamamlandi")
            {
                TempData["ErrorMessage"] = "Tamamlanmış bir proje iptal edilemez.";
            }
            else
            {
                await _projeService.UpdateStatusAsync(id, "Iptal");
                
                // Proje durumu değiştiği için bildirim gönder
                proje.Status = "Iptal"; // Bildirim servisi için durumu güncelle
                await _bildirimService.ProjeIlerlemesiDegistiBildirimiGonder(proje);
                
                TempData["SuccessMessage"] = "Proje iptal edildi.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> ProjeExists(int id)
        {
            return await _projeService.GetByIdAsync(id) != null;
        }

        private async Task LoadDropdownDataAsync()
        {
            ViewBag.Kategoriler = new SelectList(await _kategoriRepository.GetAllAsync(), "Id", "Ad");
            ViewBag.Ogrenciler = new SelectList(await _ogrenciService.GetAllAsync(), "Id", "Ad");
            ViewBag.Akademisyenler = new SelectList(await _akademisyenService.GetAllAsync(), "Id", "Ad");
            ViewBag.Durumlar = new SelectList(new[] { "Beklemede", "Atanmis", "Devam", "Tamamlandi", "Iptal" });
        }
    }
} 