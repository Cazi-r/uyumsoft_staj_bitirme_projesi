using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using System.Linq;

namespace UniversiteProjeYonetimSistemi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AuthService _authService;
    private readonly IProjeService _projeService;
    private readonly ApplicationDbContext _context;
    private readonly IOgrenciService _ogrenciService;
    private readonly IAkademisyenService _akademisyenService;

    public HomeController(
        ILogger<HomeController> logger,
        AuthService authService,
        IProjeService projeService,
        ApplicationDbContext context,
        IOgrenciService ogrenciService,
        IAkademisyenService akademisyenService)
    {
        _logger = logger;
        _authService = authService;
        _projeService = projeService;
        _context = context;
        _ogrenciService = ogrenciService;
        _akademisyenService = akademisyenService;
    }

    // Ana sayfa: Giris yoksa "Welcome", varsa role gore (Admin/Akademisyen/Ogrenci) farkli dashboard.
    public async Task<IActionResult> Index()
    {
        // Kullanıcı giriş yapmamışsa standart sayfayı göster
        if (!User.Identity.IsAuthenticated)
        {
            return View("Welcome");
        }

        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        // Kullanıcı rolüne göre farklı dashboard'ları göster
        ViewBag.UserRole = userRole;
        
        switch (userRole)
        {
            case "Admin":
                // Admin dashboard verileri
                ViewBag.KullaniciSayisi = await _context.Kullanicilar.CountAsync();
                ViewBag.ProjeSayisi = await _context.Projeler.CountAsync();
                ViewBag.OgrenciSayisi = await _context.Ogrenciler.CountAsync();
                ViewBag.AkademisyenSayisi = await _context.Akademisyenler.CountAsync();
                
                // Proje durum istatistikleri
                ViewBag.BeklemedeProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Beklemede");
                ViewBag.AtanmisProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Atanmis");
                ViewBag.DevamEdenProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Devam");
                ViewBag.TamamlananProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Tamamlandi");
                ViewBag.IptalProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Iptal");
                
                // Son etkinlikler
                ViewBag.SonEtkinlikler = await GetEtkinlikListesi(5);
                
                return View("AdminDashboard");
                
            case "Akademisyen":
                // Akademisyen dashboard verileri
                var akademisyen = await _authService.GetCurrentAkademisyenAsync();
                if (akademisyen != null)
                {
                    ViewBag.DanismanlikProjeleri = await _context.Projeler
                        .Where(p => p.MentorId == akademisyen.Id && p.Status == "Devam")
                        .Include(p => p.Ogrenci)
                        .Include(p => p.Kategori)
                        .OrderByDescending(p => p.OlusturmaTarihi)
                        .Take(5)
                        .ToListAsync();
                        
                    ViewBag.DanismanlikSayisi = await _context.Projeler
                        .CountAsync(p => p.MentorId == akademisyen.Id);
                    
                    // Bekleyen değerlendirmeler
                    ViewBag.BekleyenDegerlendirmeSayisi = await _context.Projeler
                        .Where(p => p.MentorId == akademisyen.Id && p.Status == "Devam")
                        .CountAsync(p => !_context.Degerlendirmeler.Any(d => d.ProjeId == p.Id && d.AkademisyenId == akademisyen.Id));
                    
                    // Yaklaşan teslim tarihleri
                    var bugun = DateTime.Today;
                    var birHaftaSonra = bugun.AddDays(7);
                    
                    ViewBag.YaklasanTeslimler = await _context.Projeler
                        .Where(p => p.MentorId == akademisyen.Id && 
                                   p.TeslimTarihi.HasValue && 
                                   p.TeslimTarihi >= bugun && 
                                   p.TeslimTarihi <= birHaftaSonra)
                        .Include(p => p.Ogrenci)
                        .OrderBy(p => p.TeslimTarihi)
                        .ToListAsync();
                    
                    // Son yorumlar
                    var akademisyenProjeleri = await _context.Projeler
                        .Where(p => p.MentorId == akademisyen.Id)
                        .Select(p => p.Id)
                        .ToListAsync();
                        
                    ViewBag.SonYorumlar = await _context.ProjeYorumlari
                        .Where(y => akademisyenProjeleri.Contains(y.ProjeId))
                        .Include(y => y.Proje)
                        .Include(y => y.Ogrenci)
                        .Include(y => y.Akademisyen)
                        .OrderByDescending(y => y.OlusturmaTarihi)
                        .Take(5)
                        .ToListAsync();
                        
                    // Bekleyen görüşme taleplerini getir (Index metodu için)
                    var bekleyenGorusmeler = await _context.DanismanlikGorusmeleri
                        .Include(g => g.Ogrenci)
                        .Include(g => g.Proje)
                        .Where(g => g.AkademisyenId == akademisyen.Id && g.Durum == GorusmeDurumu.HocaOnayiBekliyor)
                        .OrderBy(g => g.GorusmeTarihi)
                        .ToListAsync();
                    
                    ViewBag.BekleyenGorusmeler = bekleyenGorusmeler;
                    
                    // Danışmanlık projelerini de Index metodu için set edelim
                    var danismanlikProjeleri = await _context.Projeler
                        .Include(p => p.Ogrenci)
                        .Include(p => p.Kategori)
                        .Where(p => p.MentorId == akademisyen.Id && p.Status == "Devam")
                        .OrderByDescending(p => p.OlusturmaTarihi)
                        .Take(5)
                        .ToListAsync();
                    
                    ViewBag.DanismanlikProjeleri = danismanlikProjeleri;
                }
                return View("AkademisyenDashboard");
                
            case "Ogrenci":
                // Öğrenci dashboard verileri
                var ogrenci = await _authService.GetCurrentOgrenciAsync();
                if (ogrenci != null)
                {
                    ViewBag.Projeler = await _context.Projeler
                        .Where(p => p.OgrenciId == ogrenci.Id)
                        .Include(p => p.Mentor)
                        .Include(p => p.Kategori)
                        .OrderByDescending(p => p.OlusturmaTarihi)
                        .ToListAsync();
                        
                    // Toplam proje sayısı
                    ViewBag.ToplamProjeSayisi = await _context.Projeler
                        .CountAsync(p => p.OgrenciId == ogrenci.Id);
                    
                    // Yaklaşan teslim tarihleri
                    var bugun = DateTime.Today;
                    var birHaftaSonra = bugun.AddDays(7);
                    
                    ViewBag.YaklasanTeslimSayisi = await _context.Projeler
                        .CountAsync(p => p.OgrenciId == ogrenci.Id && 
                                  p.TeslimTarihi.HasValue && 
                                  p.TeslimTarihi >= bugun && 
                                  p.TeslimTarihi <= birHaftaSonra);
                    
                    // Yeni mesaj/yorum sayısı
                    ViewBag.YeniYorumSayisi = await _context.ProjeYorumlari
                        .CountAsync(y => y.AkademisyenId != null && 
                                   y.Proje.OgrenciId == ogrenci.Id &&
                                   y.OlusturmaTarihi >= DateTime.Now.AddDays(-7));
                    
                    // Son yorumlar
                    ViewBag.SonYorumlar = await _context.ProjeYorumlari
                        .Where(y => y.AkademisyenId != null && y.Proje.OgrenciId == ogrenci.Id)
                        .Include(y => y.Proje)
                        .Include(y => y.Akademisyen)
                        .OrderByDescending(y => y.OlusturmaTarihi)
                        .Take(3)
                        .ToListAsync();
                }
                return View("OgrenciDashboard");
                
            default:
                return View();
        }
    }

    // Akademisyen ozel dashboard; mentorluk bilgileri, bekleyen degerlendirmeler ve gorusmelerin ozetleri.
    public async Task<IActionResult> AkademisyenDashboard()
    {
        var akademisyen = await _akademisyenService.GetAkademisyenByUserName(User.Identity.Name);

        if (akademisyen == null)
            return RedirectToAction("Index");

        // Akademisyenin mentörlük yaptığı projeleri getir
        var projeler = await _context.Projeler
            .Include(p => p.Ogrenci)
            .Include(p => p.Kategori)
            .Where(p => p.MentorId == akademisyen.Id)
            .ToListAsync();

        // Danışmanlık sayısını ViewBag'e ekle
        ViewBag.DanismanlikSayisi = projeler.Count;
        
        // Değerlendirme sayıları
        ViewBag.DegerlendirmeSayisi = await _context.Projeler
            .Where(p => p.MentorId == akademisyen.Id)
            .SelectMany(p => p.Degerlendirmeler)
            .CountAsync();
            
        // Bekleyen değerlendirmeleri hesapla
        var projeDeğerlendirmeSayisi = await _context.Projeler
            .Where(p => p.MentorId == akademisyen.Id && p.Status == "Devam")
            .CountAsync(p => !p.Degerlendirmeler.Any());
                
        // Tamamlanmış ama değerlendirilmemiş aşama sayısını getir
        var degerlendirilmeyenAsamaSayisi = await _projeService.GetDegerlendirilmeyenAsamaSayisiByMentorIdAsync(akademisyen.Id);
            
        // Toplam bekleyen değerlendirme sayısı (proje + aşama)
        ViewBag.BekleyenDegerlendirmeSayisi = projeDeğerlendirmeSayisi + degerlendirilmeyenAsamaSayisi;
            
        // Değerlendirilmemiş aşamalar listesini getir
        var degerlendirilmeyenAsamalar = await _context.ProjeAsamalari
            .Include(a => a.Proje)
            .ThenInclude(p => p.Ogrenci)
            .Where(a => a.Proje.MentorId == akademisyen.Id && a.Tamamlandi && !a.Degerlendirildi)
            .OrderByDescending(a => a.TamamlanmaTarihi)
            .Take(5)
            .ToListAsync();
                
        ViewBag.DegerlendirilmeyenAsamalar = degerlendirilmeyenAsamalar;

        // Son bildirimleri getir
        var bildirimler = await _context.Bildirimler
            .Where(b => b.AkademisyenId == akademisyen.Id)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();
        
        // Bugünün tarihini al
        var bugun = DateTime.Now;
        // Bir hafta sonrasını hesapla
        var birHaftaSonra = bugun.AddDays(7);
        
        // Bekleyen görüşme taleplerini getir (tümünü, sadece öğrencilerden değil)
        var bekleyenTalepler = await _context.DanismanlikGorusmeleri
            .Include(g => g.Ogrenci)
            .Include(g => g.Proje)
            .Where(g => g.AkademisyenId == akademisyen.Id && g.Durum == GorusmeDurumu.HocaOnayiBekliyor)
            .OrderBy(g => g.GorusmeTarihi)
            .ToListAsync(); // Limit kaldırıldı, tümünü getiriyoruz
            
        // Tüm görüşmeleri getir ve ZamanDurumu'nu güncelle
        var tumGorusmeler = await _context.DanismanlikGorusmeleri
            .Include(g => g.Ogrenci)
            .Include(g => g.Proje)
            .Where(g => g.AkademisyenId == akademisyen.Id)
            .ToListAsync();
            
        foreach (var gorusme in tumGorusmeler)
        {
            gorusme.GuncelleZamanDurumu();
        }
            
        // Yakın Görüşmeleri getir (onaylanmış ve bugün veya yakın gelecekte)
        var yakinGorusmeler = tumGorusmeler
            .Where(g => g.Durum == GorusmeDurumu.Onaylandi && (g.ZamanDurumu == "Bugun" || g.ZamanDurumu == "YakinGelecek"))
            .OrderBy(g => g.GorusmeTarihi)
            .Take(3)
            .ToList();
            
        // Uzak gelecek görüşmeleri getir (onaylanmış ve uzak gelecekte)
        var gelecekGorusmeler = tumGorusmeler
            .Where(g => g.Durum == GorusmeDurumu.Onaylandi && g.ZamanDurumu == "UzakGelecek")
            .OrderBy(g => g.GorusmeTarihi)
            .Take(3)
            .ToList();
            
        // Geçmiş görüşmeleri getir (onaylanmış ve tarihi geçmiş)
        var gecmisGorusmeler = tumGorusmeler
            .Where(g => g.Durum == GorusmeDurumu.Onaylandi && g.ZamanDurumu == "Gecmis")
            .OrderByDescending(g => g.GorusmeTarihi)
            .Take(3)
            .ToList();
            
        // İptal edilmiş görüşmeleri getir
        var iptalGorusmeler = await _context.DanismanlikGorusmeleri
            .Include(g => g.Ogrenci)
            .Include(g => g.Proje)
            .Where(g => g.AkademisyenId == akademisyen.Id && g.Durum == GorusmeDurumu.IptalEdildi)
            .OrderByDescending(g => g.GorusmeTarihi)
            .Take(3)
            .ToListAsync();

        ViewBag.Projeler = projeler;
        ViewBag.Bildirimler = bildirimler;
        ViewBag.BekleyenGorusmeler = bekleyenTalepler;
        ViewBag.YakinGorusmeler = yakinGorusmeler;
        ViewBag.GelecekGorusmeler = gelecekGorusmeler;
        ViewBag.GecmisGorusmeler = gecmisGorusmeler;
        ViewBag.IptalGorusmeler = iptalGorusmeler;

        return View();
    }

    // Ogrenci ozel dashboard; projeleri, bildirimleri ve son gorusmeleri listeler.
    public async Task<IActionResult> OgrenciDashboard()
    {
        var ogrenci = await _ogrenciService.GetOgrenciByUserName(User.Identity.Name);

        if (ogrenci == null)
            return RedirectToAction("Index");

        // Öğrencinin projelerini getir
        var projeler = await _context.Projeler
            .Include(p => p.Mentor)
            .Include(p => p.Kategori)
            .Where(p => p.OgrenciId == ogrenci.Id)
            .ToListAsync();

        // Son bildirimleri getir
        var bildirimler = await _context.Bildirimler
            .Where(b => b.OgrenciId == ogrenci.Id)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Son görüşmeleri getir
        var gorusmeler = await _context.DanismanlikGorusmeleri
            .Include(g => g.Akademisyen)
            .Where(g => g.OgrenciId == ogrenci.Id)
            .OrderByDescending(g => g.GorusmeTarihi)
            .Take(3)
            .ToListAsync();

        ViewBag.Projeler = projeler;
        ViewBag.Bildirimler = bildirimler;
        ViewBag.Gorusmeler = gorusmeler;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    private async Task<List<dynamic>> GetEtkinlikListesi(int limit = 50)
    {
        var etkinlikler = new List<dynamic>();
        
        // Giriş bilgileri
        var sonGirisler = await _context.Kullanicilar
            .Where(k => k.SonGiris.HasValue)
            .Select(k => new {
                Id = k.Id,
                Ad = k.Ad,
                Soyad = k.Soyad,
                Email = k.Email,
                Rol = k.Rol,
                Tarih = k.SonGiris.Value,
                Tip = "Giriş"
            })
            .OrderByDescending(k => k.Tarih)
            .Take(limit)
            .ToListAsync();
        
        foreach (var giris in sonGirisler)
        {
            etkinlikler.Add(new {
                KullaniciId = giris.Id,
                KullaniciAdi = $"{giris.Ad} {giris.Soyad}",
                KullaniciEmail = giris.Email,
                KullaniciRol = giris.Rol,
                Islem = "Sistem girişi yaptı",
                Tarih = giris.Tarih,
                Tip = "Giriş"
            });
        }
        
        // Proje oluşturma etkinlikleri
        var projeler = await _context.Projeler
            .Include(p => p.Ogrenci)
            .ThenInclude(o => o.Kullanici)
            .Select(p => new {
                KullaniciId = p.Ogrenci.KullaniciId,
                KullaniciAdi = $"{p.Ogrenci.Ad} {p.Ogrenci.Soyad}",
                KullaniciEmail = p.Ogrenci.Email,
                KullaniciRol = "Ogrenci",
                Islem = $"\"{p.Ad}\" adlı projeyi oluşturdu",
                Tarih = p.OlusturmaTarihi,
                Tip = "Proje"
            })
            .OrderByDescending(p => p.Tarih)
            .Take(limit)
            .ToListAsync();
            
        etkinlikler.AddRange(projeler);
        
        // Tüm etkinlikleri tarihe göre sırala ve istenen sayıda döndür
        return etkinlikler.OrderByDescending(e => (dynamic)e.Tarih).Take(limit).ToList();
    }
}
