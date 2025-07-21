using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using System.Linq;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Admin,Akademisyen")]
    public class KategoriController : Controller
    {
        private readonly IRepository<ProjeKategori> _kategoriRepository;
        private readonly IProjeService _projeService;

        public KategoriController(
            IRepository<ProjeKategori> kategoriRepository,
            IProjeService projeService)
        {
            _kategoriRepository = kategoriRepository;
            _projeService = projeService;
        }

        // GET: Kategori
        [AllowAnonymous] // Öğrenciler de dahil herkes görebilsin
        public async Task<IActionResult> Index()
        {
            var kategoriler = await _kategoriRepository.GetAllAsync();
            return View(kategoriler);
        }

        // GET: Kategori/Details/5
        [AllowAnonymous] // Öğrenciler de dahil herkes görebilsin
        public async Task<IActionResult> Details(int id)
        {
            var kategori = await _kategoriRepository.GetByIdWithIncludeAsync(id, k => k.Projeler);
            if (kategori == null)
            {
                return NotFound();
            }

            return View(kategori);
        }

        // GET: Kategori/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kategori/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjeKategori kategori)
        {
            if (ModelState.IsValid)
            {
                kategori.CreatedAt = DateTime.Now;
                kategori.UpdatedAt = DateTime.Now;
                await _kategoriRepository.AddAsync(kategori);
                TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(kategori);
        }

        // GET: Kategori/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var kategori = await _kategoriRepository.GetByIdAsync(id);
            if (kategori == null)
            {
                return NotFound();
            }
            return View(kategori);
        }

        // POST: Kategori/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjeKategori kategori)
        {
            if (id != kategori.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKategori = await _kategoriRepository.GetByIdAsync(id);
                    if (existingKategori == null)
                    {
                        return NotFound();
                    }

                    existingKategori.Ad = kategori.Ad;
                    existingKategori.Aciklama = kategori.Aciklama;
                    existingKategori.Renk = kategori.Renk;
                    existingKategori.UpdatedAt = DateTime.Now;

                    await _kategoriRepository.UpdateAsync(existingKategori);
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KategoriExists(kategori.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(kategori);
        }

        // GET: Kategori/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var kategori = await _kategoriRepository.GetByIdWithIncludeAsync(id, k => k.Projeler);
            if (kategori == null)
            {
                return NotFound();
            }

            return View(kategori);
        }

        // POST: Kategori/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategori = await _kategoriRepository.GetByIdAsync(id);
            if (kategori == null)
            {
                return NotFound();
            }

            // Önce kategoriye bağlı projeleri kontrol et
            var projeler = await _projeService.GetByKategoriIdAsync(id);
            foreach (var proje in projeler)
            {
                proje.KategoriId = null; // Kategori bağlantısını kaldır
                await _projeService.UpdateAsync(proje);
            }

            await _kategoriRepository.DeleteAsync(kategori);
            TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> KategoriExists(int id)
        {
            var kategori = await _kategoriRepository.GetByIdAsync(id);
            return kategori != null;
        }
    }
} 