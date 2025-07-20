using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public ProjeController(
            IProjeService projeService,
            IOgrenciService ogrenciService,
            IAkademisyenService akademisyenService,
            IRepository<ProjeKategori> kategoriRepository)
        {
            _projeService = projeService;
            _ogrenciService = ogrenciService;
            _akademisyenService = akademisyenService;
            _kategoriRepository = kategoriRepository;
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

            return View(proje);
        }

        // GET: Proje/Create
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownDataAsync();
            return View();
        }

        // POST: Proje/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Create(Proje proje)
        {
            if (ModelState.IsValid)
            {
                await _projeService.AddAsync(proje);
                return RedirectToAction(nameof(Index));
            }
            
            await LoadDropdownDataAsync();
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

            ViewBag.Ogrenciler = new SelectList(await _ogrenciService.GetAllAsync(), "Id", "Ad");
            ViewBag.Akademisyenler = new SelectList(await _akademisyenService.GetAllAsync(), "Id", "Ad");
            
            return View(proje);
        }

        // POST: Proje/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Akademisyen")]
        public async Task<IActionResult> Assign(int id, int ogrenciId, int mentorId)
        {
            if (ogrenciId > 0)
            {
                await _projeService.AssignToOgrenciAsync(id, ogrenciId);
            }
            
            if (mentorId > 0)
            {
                await _projeService.AssignToMentorAsync(id, mentorId);
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