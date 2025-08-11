using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniversiteProjeYonetimSistemi.Services;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Ogrenci")]
    public class OgrenciController : Controller
    {
        private readonly IOgrenciService _ogrenciService;
        private readonly IProjeService _projeService;

        public OgrenciController(IOgrenciService ogrenciService, IProjeService projeService)
        {
            _ogrenciService = ogrenciService;
            _projeService = projeService;
        }

        // Ogrencinin kendi projelerini listeler. [Authorize(Roles = "Ogrenci")] ile yalnizca Ogrenci rolune aciktir.
        // Durum filtresi ve siralama parametreleri ile listeyi ozellestirir, istatistikleri ViewBag'e koyar.
        public async Task<IActionResult> Projeler(string durum = "Tumu", string sirala = "Yeni")
        {
            // Giriş yapmış kullanıcının ID'sini al
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Kullanıcı ID'sine göre öğrenci bilgilerini getir
            var ogrenci = await _ogrenciService.GetByKullaniciIdAsync(userId);
            
            if (ogrenci == null)
            {
                return NotFound("Öğrenci bilgileri bulunamadı.");
            }
            
            // Öğrencinin projelerini detaylarıyla birlikte getir
            var projeler = await _ogrenciService.GetProjelerWithDetailsAsync(ogrenci.Id);
            
            // Duruma göre filtrele
            if (durum != "Tumu")
            {
                projeler = projeler.Where(p => p.Status == durum).ToList();
            }
            
            // Sıralama yap
            projeler = sirala switch
            {
                "Yeni" => projeler.OrderByDescending(p => p.OlusturmaTarihi).ToList(),
                "Eski" => projeler.OrderBy(p => p.OlusturmaTarihi).ToList(),
                "Teslim" => projeler.OrderBy(p => p.TeslimTarihi ?? DateTime.MaxValue).ToList(),
                "Ad" => projeler.OrderBy(p => p.Ad).ToList(),
                _ => projeler.OrderByDescending(p => p.OlusturmaTarihi).ToList()
            };
            
            // İstatistikleri hesapla
            ViewBag.ToplamProjeSayisi = projeler.Count();
            ViewBag.DevamEdenProjeSayisi = projeler.Count(p => p.Status == "Devam");
            ViewBag.TamamlananProjeSayisi = projeler.Count(p => p.Status == "Tamamlandi");
            ViewBag.BeklemedeProjeSayisi = projeler.Count(p => p.Status == "Beklemede" || p.Status == "Atanmis");
            
            // Ek istatistikler
            ViewBag.ToplamTeslimSayisi = projeler.SelectMany(p => p.Dosyalar ?? new List<ProjeDosya>()).Count();
            ViewBag.TamamlananAsamaSayisi = projeler.SelectMany(p => p.Asamalar ?? new List<ProjeAsamasi>()).Count(a => a.Tamamlandi);
            ViewBag.YaklasanAsamalar = projeler.SelectMany(p => p.Asamalar ?? new List<ProjeAsamasi>())
                .Where(a => !a.Tamamlandi && a.BitisTarihi.HasValue && a.BitisTarihi.Value >= DateTime.Now && a.BitisTarihi.Value <= DateTime.Now.AddDays(7))
                .OrderBy(a => a.BitisTarihi)
                .ToList();
            
            // ViewBag ile öğrenci bilgilerini ve filtreleme/sıralama seçeneklerini gönder
            ViewBag.Ogrenci = ogrenci;
            ViewBag.SeciliDurum = durum;
            ViewBag.SeciliSiralama = sirala;
            
            return View(projeler);
        }
    }
} 