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

        // Dosya ekleme formu: Proje ve kullanici yetkisi kontrol edilir (Admin, projenin ogrencisi veya danismani).
        // Proje adi ViewBag'e set edilir.
        public async Task<IActionResult> Add(int projeId)
        {
            // Proje var mı kontrol et
            var proje = await _projeService.GetByIdAsync(projeId);
            if (proje == null)
            {
                TempData["ErrorMessage"] = "Proje bulunamadı.";
                return RedirectToAction("Index", "Proje");
            }

            // Kullanıcının bu projeye dosya yükleme yetkisi var mı kontrol et
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

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
                TempData["ErrorMessage"] = "Bu projeye dosya yükleme yetkiniz bulunmuyor.";
                return RedirectToAction("Details", "Proje", new { id = projeId });
            }

            var model = new ProjeDosyaViewModel
            {
                ProjeId = projeId
            };

            ViewBag.ProjeAdi = proje.Ad;
            return View(model);
        }

        // Dosya ekleme POST: CSRF korumali. Boyut/tur kontrolleri yapilir, yetkili ise dosya yuklenir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProjeDosyaViewModel model)
        {
            // Model validation
            if (!ModelState.IsValid)
            {
                var proje = await _projeService.GetByIdAsync(model.ProjeId);
                ViewBag.ProjeAdi = proje?.Ad;
                return View(model);
            }

            // Proje var mı kontrol et
            var projeCheck = await _projeService.GetByIdAsync(model.ProjeId);
            if (projeCheck == null)
            {
                TempData["ErrorMessage"] = "Proje bulunamadı.";
                return RedirectToAction("Index", "Proje");
            }

            // Kullanıcının bu projeye dosya yükleme yetkisi var mı kontrol et
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            bool hasPermission = false;
            int? yukleyenId = null;
            string yukleyenTipi = "Ogrenci";

            if (User.IsInRole("Admin"))
            {
                hasPermission = true;
                // Admin için default olarak akademisyen olarak işaretle
                yukleyenTipi = "Akademisyen";
            }
            else if (User.IsInRole("Ogrenci"))
            {
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                hasPermission = ogrenci != null && projeCheck.OgrenciId == ogrenci.Id;
                if (hasPermission)
                {
                    yukleyenId = ogrenci.Id;
                    yukleyenTipi = "Ogrenci";
                }
            }
            else if (User.IsInRole("Akademisyen"))
            {
                hasPermission = await IsCurrentUserProjectMentor(model.ProjeId);
                if (hasPermission)
                {
                    var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                    yukleyenId = akademisyen?.Id;
                    yukleyenTipi = "Akademisyen";
                }
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu projeye dosya yükleme yetkiniz bulunmuyor.";
                return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
            }

            // Dosya var mı ve geçerli mi kontrol et
            if (model.Dosya != null && model.Dosya.Length > 0)
            {
                try
                {
                    // Dosya boyutu kontrolü (50MB)
                    if (model.Dosya.Length > 50 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Dosya boyutu 50MB'dan büyük olamaz.";
                        ViewBag.ProjeAdi = projeCheck.Ad;
                        return View(model);
                    }

                    // Dosya türü kontrolü
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", 
                        ".jpg", ".jpeg", ".png", ".gif", ".xlsx", ".xls", ".ppt", ".pptx" };
                    var extension = Path.GetExtension(model.Dosya.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Bu dosya türü desteklenmiyor.";
                        ViewBag.ProjeAdi = projeCheck.Ad;
                        return View(model);
                    }

                    // Dosyayı yükle
                    await _projeService.UploadFileAsync(model.ProjeId, model.Dosya, model.Aciklama, yukleyenId, yukleyenTipi);
                    TempData["SuccessMessage"] = "Dosya başarıyla yüklendi.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Dosya yüklenirken bir hata oluştu: " + ex.Message;
                    ViewBag.ProjeAdi = projeCheck.Ad;
                    return View(model);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                ViewBag.ProjeAdi = projeCheck.Ad;
                return View(model);
            }

            return RedirectToAction("Details", "Proje", new { id = model.ProjeId });
        }

        // Dosya indirme: Erişim yetkisi (Admin/Ogrenci-sahip/Akademisyen-danisman) kontrol edilir, uygun content type ile dosya dondurulur.
        public async Task<IActionResult> Download(int id)
        {
            var dosya = await _projeService.GetFileByIdAsync(id);
            if (dosya == null)
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return NotFound();
            }

            // Kullanıcının bu dosyayı indirme yetkisi var mı kontrol et
            var proje = await _projeService.GetByIdAsync(dosya.ProjeId);
            if (proje == null)
            {
                TempData["ErrorMessage"] = "Proje bulunamadı.";
                return NotFound();
            }

            // Yetki kontrolü
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
                hasPermission = await IsCurrentUserProjectMentor(dosya.ProjeId);
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu dosyayı indirme yetkiniz bulunmuyor.";
                return RedirectToAction("Details", "Proje", new { id = dosya.ProjeId });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dosya.DosyaYolu.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Dosya fiziksel olarak bulunamadı.";
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Dosya türüne göre content type belirle
            string contentType = GetContentType(dosya.DosyaTipi, dosya.DosyaAdi);

            return File(memory, contentType, dosya.DosyaAdi);
        }

        private string GetContentType(string dosyaTipi, string dosyaAdi)
        {
            if (!string.IsNullOrEmpty(dosyaTipi))
            {
                return dosyaTipi;
            }

            // Eğer content type bilinmiyorsa, dosya uzantısına göre belirle
            var extension = Path.GetExtension(dosyaAdi).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        // Projeye ait dosyalari listeler; erisim yetkisi rol ve sahiplik/danismanlik durumuna gore kontrol edilir.
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

            var dosyalar = await _projeService.GetFilesByProjeIdAsync(projeId);
            
            // Yükleyen kişilerin adlarını al
            var dosyalarWithYukleyen = new List<object>();
            foreach (var dosya in dosyalar)
            {
                string yukleyenAdi = "Bilinmiyor";
                if (dosya.YukleyenId.HasValue)
                {
                    if (dosya.YukleyenTipi == "Ogrenci")
                    {
                        var ogrenci = await _authService.GetOgrenciByIdAsync(dosya.YukleyenId.Value);
                        if (ogrenci != null)
                        {
                            yukleyenAdi = $"{ogrenci.Ad} {ogrenci.Soyad}";
                        }
                    }
                    else if (dosya.YukleyenTipi == "Akademisyen")
                    {
                        var akademisyen = await _authService.GetAkademisyenByIdAsync(dosya.YukleyenId.Value);
                        if (akademisyen != null)
                        {
                            yukleyenAdi = $"{akademisyen.Ad} {akademisyen.Soyad}";
                        }
                    }
                }

                dosyalarWithYukleyen.Add(new
                {
                    Dosya = dosya,
                    YukleyenAdi = yukleyenAdi
                });
            }

            ViewBag.DosyalarWithYukleyen = dosyalarWithYukleyen;
            return View(dosyalar);
        }

        // Tek dosya detayini gosterir; yukleyen kisinin adini ViewBag'e koyar, yetki kontrolu yapar.
        public async Task<IActionResult> Details(int id)
        {
            var dosya = await _projeService.GetFileByIdAsync(id);
            if (dosya == null)
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return NotFound();
            }

            // Proje var mı kontrol et
            var proje = await _projeService.GetByIdAsync(dosya.ProjeId);
            if (proje == null)
            {
                TempData["ErrorMessage"] = "Proje bulunamadı.";
                return NotFound();
            }

            // Yükleyen kişinin bilgilerini al
            string yukleyenAdi = "Bilinmiyor";
            if (dosya.YukleyenId.HasValue)
            {
                if (dosya.YukleyenTipi == "Ogrenci")
                {
                    var ogrenci = await _authService.GetOgrenciByIdAsync(dosya.YukleyenId.Value);
                    if (ogrenci != null)
                    {
                        yukleyenAdi = $"{ogrenci.Ad} {ogrenci.Soyad}";
                    }
                }
                else if (dosya.YukleyenTipi == "Akademisyen")
                {
                    var akademisyen = await _authService.GetAkademisyenByIdAsync(dosya.YukleyenId.Value);
                    if (akademisyen != null)
                    {
                        yukleyenAdi = $"{akademisyen.Ad} {akademisyen.Soyad}";
                    }
                }
            }

            ViewBag.YukleyenAdi = yukleyenAdi;

            // Kullanıcının bu dosyaya erişim yetkisi var mı kontrol et
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
                hasPermission = await IsCurrentUserProjectMentor(dosya.ProjeId);
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Bu dosyaya erişim yetkiniz bulunmuyor.";
                return RedirectToAction("Index", "Proje");
            }

            return View(dosya);
        }

        // Dosya silme onayi: Sadece projenin danismani veya admin gorebilir; yukleyen bilgisini ViewBag'e ekler.
        public async Task<IActionResult> Delete(int id)
        {
            var dosya = await _projeService.GetFileByIdAsync(id);
            if (dosya == null)
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return NotFound();
            }

            // Yükleyen kişinin bilgilerini al
            string yukleyenAdi = "Bilinmiyor";
            if (dosya.YukleyenId.HasValue)
            {
                if (dosya.YukleyenTipi == "Ogrenci")
                {
                    var ogrenci = await _authService.GetOgrenciByIdAsync(dosya.YukleyenId.Value);
                    if (ogrenci != null)
                    {
                        yukleyenAdi = $"{ogrenci.Ad} {ogrenci.Soyad}";
                    }
                }
                else if (dosya.YukleyenTipi == "Akademisyen")
                {
                    var akademisyen = await _authService.GetAkademisyenByIdAsync(dosya.YukleyenId.Value);
                    if (akademisyen != null)
                    {
                        yukleyenAdi = $"{akademisyen.Ad} {akademisyen.Soyad}";
                    }
                }
            }

            ViewBag.YukleyenAdi = yukleyenAdi;

            // Sadece projenin danışmanı dosya silebilir
            if (!await IsCurrentUserProjectMentor(dosya.ProjeId))
            {
                TempData["ErrorMessage"] = "Bu dosyayı sadece projenin danışmanı veya admin silebilir.";
                return RedirectToAction("Index", "ProjeDosya", new { projeId = dosya.ProjeId });
            }

            return View(dosya);
        }

        // Dosya silme POST: yalnizca projenin danismani veya admin silebilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Delete(int id, int projeId)
        {
            // Sadece projenin danışmanı dosya silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu projeye ait dosyaları sadece danışmanı veya admin silebilir.";
                return RedirectToAction("Index", "ProjeDosya", new { projeId = projeId });
            }

            await _projeService.DeleteFileAsync(id);
            TempData["SuccessMessage"] = "Dosya başarıyla silindi.";
            return RedirectToAction("Index", "ProjeDosya", new { projeId = projeId });
        }
    }
} 