using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Models.ViewModels;
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

        public ProjeController(
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IRepository<ProjeKategori> kategoriRepository,
            AuthService authService)
        {
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _kategoriRepository = kategoriRepository;
            _authService = authService;
        }

        // GET: Proje
        public async Task<IActionResult> Index()
        {
            var projeler = await _projeService.GetAllAsync();
            return View(projeler);
        }

        // GET: Proje/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            // Şu anki kullanıcının projenin danışmanı olup olmadığını kontrol et
            ViewBag.IsCurrentUserMentor = await IsCurrentUserProjectMentor(id);

            return View(proje);
        }

        // GET: Proje/Create
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

        // POST: Proje/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> Create(Proje proje)
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
                        proje.Status = "Beklemede"; // Öğrenci oluşturduğunda durumu "Beklemede" olarak ayarla
                    }
                }
                
                await _projeService.AddAsync(proje);
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

        // GET: Proje/Edit/5
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            await LoadDropdownDataAsync();
            return View(proje);
        }

        // POST: Proje/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Edit(int id, Proje proje)
        {
            if (id != proje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _projeService.UpdateAsync(proje);
                    return RedirectToAction(nameof(Index));
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

        // GET: Proje/Delete/5
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

        // POST: Proje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _projeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Proje/Assign/5
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Assign(int id)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            var ogrenciler = await _ogrenciService.GetAllAsync();
            var akademisyenler = await _akademisyenService.GetAllAsync();

            ViewBag.Ogrenciler = new SelectList(ogrenciler.Select(o => new { Id = o.Id, AdSoyad = $"{o.Ad} {o.Soyad}" }), "Id", "AdSoyad");
            ViewBag.Akademisyenler = new SelectList(akademisyenler.Select(a => new { Id = a.Id, AdSoyad = $"{a.Unvan} {a.Ad} {a.Soyad}" }), "Id", "AdSoyad");
            
            return View(proje);
        }

        // POST: Proje/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Assign(int id, int ogrenciId, int mentorId)
        {
            var proje = await _projeService.GetByIdAsync(id);
            if (proje == null)
            {
                return NotFound();
            }
            
            // Mevcut öğrenciyi koru, sadece form ile gönderilen öğrenci ID'sini kullan
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
            
            return RedirectToAction(nameof(Details), new { id });
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

        // POST: Proje/UpdateStatusToInProgress/5
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
                TempData["SuccessMessage"] = "Proje durumu 'Devam Ediyor' olarak güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Proje durumu güncellenemedi. Proje durumu 'Atanmış' olmalıdır.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
        
        // POST: Proje/CompleteProject/5
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
                TempData["SuccessMessage"] = "Proje başarıyla tamamlandı olarak işaretlendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Proje tamamlandı olarak işaretlenemedi. Proje durumu 'Devam Ediyor' olmalıdır.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
        
        // POST: Proje/CancelProject/5
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
                TempData["SuccessMessage"] = "Proje iptal edildi.";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Proje/UploadFile/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(ProjeDosyaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction(nameof(Details), new { id = model.ProjeId });
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

            return RedirectToAction(nameof(Details), new { id = model.ProjeId });
        }

        // GET: Proje/DownloadFile/5
        public async Task<IActionResult> DownloadFile(int id)
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

        // POST: Proje/DeleteFile/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> DeleteFile(int id, int projeId)
        {
            // Sadece projenin danışmanı dosya silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu projeye ait dosyaları sadece danışmanı veya admin silebilir.";
                return RedirectToAction(nameof(Details), new { id = projeId });
            }

            await _projeService.DeleteFileAsync(id);
            TempData["SuccessMessage"] = "Dosya başarıyla silindi.";
            return RedirectToAction(nameof(Details), new { id = projeId });
        }

        // POST: Proje/AddComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(ProjeYorumViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen yorum alanını doldurun.";
                return RedirectToAction(nameof(Details), new { id = model.ProjeId });
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

            await _projeService.AddCommentAsync(model.ProjeId, model.Icerik, model.YorumTipi, ogrenciId, akademisyenId);
            TempData["SuccessMessage"] = "Yorum başarıyla eklendi.";

            return RedirectToAction(nameof(Details), new { id = model.ProjeId });
        }

        // POST: Proje/DeleteComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int projeId)
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
                return RedirectToAction(nameof(Details), new { id = projeId });
            }
            
            await _projeService.DeleteCommentAsync(id);
            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            return RedirectToAction(nameof(Details), new { id = projeId });
        }

        // GET: Proje/AddEvaluation/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> AddEvaluation(int id)
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
                return RedirectToAction(nameof(Details), new { id });
            }
            
            var model = new DegerlendirmeViewModel
            {
                ProjeId = id
            };
            
            return View(model);
        }

        // POST: Proje/AddEvaluation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> AddEvaluation(DegerlendirmeViewModel model)
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
                return RedirectToAction(nameof(Details), new { id = model.ProjeId });
            }

            await _projeService.AddEvaluationAsync(model.ProjeId, model.Puan, model.Aciklama, model.DegerlendirmeTipi, akademisyen.Id);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla eklendi.";

            return RedirectToAction(nameof(Details), new { id = model.ProjeId });
        }

        // POST: Proje/DeleteEvaluation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> DeleteEvaluation(int id, int projeId)
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
                return RedirectToAction(nameof(Details), new { id = projeId });
            }
            
            await _projeService.DeleteEvaluationAsync(id);
            TempData["SuccessMessage"] = "Değerlendirme başarıyla silindi.";
            
            return RedirectToAction(nameof(Details), new { id = projeId });
        }

        // GET: Proje/AddStage/5
        [HttpGet]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> AddStage(int id)
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
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new ProjeAsamasi
            {
                ProjeId = id,
                SiraNo = (await _projeService.GetStagesByProjeIdAsync(id)).Count() + 1
            };

            return View(model);
        }

        // POST: Proje/AddStage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> AddStage(ProjeAsamasi model)
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

            return RedirectToAction(nameof(Details), new { id = model.ProjeId });
        }

        // POST: Proje/UpdateStageStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen,Ogrenci")]
        public async Task<IActionResult> UpdateStageStatus(int id, int projeId, bool tamamlandi)
        {
            await _projeService.UpdateStageStatusAsync(id, tamamlandi);
            TempData["SuccessMessage"] = tamamlandi ? "Aşama tamamlandı olarak işaretlendi." : "Aşama devam ediyor olarak işaretlendi.";
            return RedirectToAction(nameof(Details), new { id = projeId });
        }

        // POST: Proje/DeleteStage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> DeleteStage(int id, int projeId)
        {
            // Sadece projenin danışmanı aşama silebilir
            if (!await IsCurrentUserProjectMentor(projeId))
            {
                TempData["ErrorMessage"] = "Bu projenin aşamalarını sadece danışmanı veya admin silebilir.";
                return RedirectToAction(nameof(Details), new { id = projeId });
            }
            
            await _projeService.DeleteStageAsync(id);
            TempData["SuccessMessage"] = "Proje aşaması başarıyla silindi.";
            return RedirectToAction(nameof(Details), new { id = projeId });
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