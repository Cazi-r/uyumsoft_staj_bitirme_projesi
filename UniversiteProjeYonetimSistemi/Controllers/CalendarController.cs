using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Ogrenci,Akademisyen")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public CalendarController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Takvim sayfasi - FullCalendar ile kullanicinin etkinliklerini gosterir
        public IActionResult Index()
        {
            return View();
        }

        // Kullaniciya ozel etkinlikleri JSON olarak dondurur (FullCalendar events kaynagi)
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var events = new List<object>();

            if (currentUser.Rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == currentUser.Id);
                if (ogrenci != null)
                {
                    // Proje teslim tarihleri
                    var projeler = await _context.Projeler
                        .Where(p => p.OgrenciId == ogrenci.Id)
                        .ToListAsync();

                    foreach (var p in projeler)
                    {
                        if (p.TeslimTarihi.HasValue)
                        {
                            events.Add(new
                            {
                                title = $"📋 Proje Teslim: {p.Ad}",
                                start = p.TeslimTarihi.Value.ToString("yyyy-MM-dd"),
                                color = "#0d6efd",
                                url = Url.Action("Details", "Proje", new { id = p.Id })
                            });
                        }
                    }

                    // Asama bitis tarihleri
                    var projeIds = projeler.Select(pr => pr.Id).ToList();
                    if (projeIds.Count > 0)
                    {
                        var asamalar = await _context.ProjeAsamalari
                            .Include(a => a.Proje)
                            .Where(a => projeIds.Contains(a.ProjeId) && a.BitisTarihi.HasValue)
                            .ToListAsync();

                        foreach (var a in asamalar)
                        {
                            events.Add(new
                            {
                                title = $"⚡ {a.Proje.Ad} - {a.AsamaAdi}",
                                start = a.BitisTarihi.Value.ToString("yyyy-MM-dd"),
                                color = "#f59e0b",
                                url = Url.Action("Details", "Proje", new { id = a.ProjeId })
                            });
                        }
                    }

                    // Danismanlik gorusmeleri
                    var gorusmeler = await _context.DanismanlikGorusmeleri
                        .Where(g => g.OgrenciId == ogrenci.Id)
                        .ToListAsync();

                    foreach (var g in gorusmeler)
                    {
                        events.Add(new
                        {
                            title = $"💬 Gorusme: {g.Baslik}",
                            start = g.GorusmeTarihi.ToString("s"), // ISO 8601
                            color = "#10b981",
                            url = Url.Action("Details", "DanismanlikGorusmesi", new { id = g.Id })
                        });
                    }
                }
            }
            else if (currentUser.Rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == currentUser.Id);
                if (akademisyen != null)
                {
                    // Mentoru olunan projelerin teslim tarihleri
                    var projeler = await _context.Projeler
                        .Include(p => p.Ogrenci)
                        .Where(p => p.MentorId == akademisyen.Id)
                        .ToListAsync();

                    foreach (var p in projeler)
                    {
                        if (p.TeslimTarihi.HasValue)
                        {
                            events.Add(new
                            {
                                title = $"📋 Proje Teslim: {p.Ad} ({p.Ogrenci?.Ad} {p.Ogrenci?.Soyad})",
                                start = p.TeslimTarihi.Value.ToString("yyyy-MM-dd"),
                                color = "#0d6efd",
                                url = Url.Action("Details", "Proje", new { id = p.Id })
                            });
                        }
                    }

                    // Asama bitis tarihleri
                    var projeIds = projeler.Select(pr => pr.Id).ToList();
                    if (projeIds.Count > 0)
                    {
                        var asamalar = await _context.ProjeAsamalari
                            .Include(a => a.Proje)
                            .ThenInclude(p => p.Ogrenci)
                            .Where(a => projeIds.Contains(a.ProjeId) && a.BitisTarihi.HasValue)
                            .ToListAsync();

                        foreach (var a in asamalar)
                        {
                            events.Add(new
                            {
                                title = $"⚡ {a.Proje.Ad} - {a.AsamaAdi} ({a.Proje.Ogrenci?.Ad} {a.Proje.Ogrenci?.Soyad})",
                                start = a.BitisTarihi.Value.ToString("yyyy-MM-dd"),
                                color = "#f59e0b",
                                url = Url.Action("Details", "Proje", new { id = a.ProjeId })
                            });
                        }
                    }

                    // Danismanlik gorusmeleri
                    var gorusmeler = await _context.DanismanlikGorusmeleri
                        .Include(g => g.Ogrenci)
                        .Where(g => g.AkademisyenId == akademisyen.Id)
                        .ToListAsync();

                    foreach (var g in gorusmeler)
                    {
                        events.Add(new
                        {
                            title = $"💬 Gorusme: {g.Baslik} ({g.Ogrenci?.Ad} {g.Ogrenci?.Soyad})",
                            start = g.GorusmeTarihi.ToString("s"),
                            color = "#10b981",
                            url = Url.Action("Details", "DanismanlikGorusmesi", new { id = g.Id })
                        });
                    }
                }
            }

            return Json(events);
        }
    }
}


