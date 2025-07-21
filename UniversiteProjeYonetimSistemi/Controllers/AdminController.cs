using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using UniversiteProjeYonetimSistemi.Services;

namespace UniversiteProjeYonetimSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly IAkademisyenService _akademisyenService;
        private readonly IOgrenciService _ogrenciService;
        private readonly IProjeService _projeService;

        public AdminController(
            ApplicationDbContext context,
            AuthService authService,
            IAkademisyenService akademisyenService,
            IOgrenciService ogrenciService,
            IProjeService projeService)
        {
            _context = context;
            _authService = authService;
            _akademisyenService = akademisyenService;
            _ogrenciService = ogrenciService;
            _projeService = projeService;
        }

        // Kullanıcı listesi - Filtreleme, Sıralama ve Arama özellikleri ile
        public async Task<IActionResult> Kullanicilar(string rol = "", string durum = "", string siralama = "id_asc", string arama = "")
        {
            ViewData["RolFilter"] = rol;
            ViewData["DurumFilter"] = durum;
            ViewData["SiralamaFilter"] = siralama;
            ViewData["AramaFilter"] = arama;
            
            var kullanicilar = _context.Kullanicilar.AsQueryable();
            
            // Rol filtresi uygula
            if (!string.IsNullOrEmpty(rol))
            {
                kullanicilar = kullanicilar.Where(k => k.Rol == rol);
            }
            
            // Durum filtresi uygula
            if (!string.IsNullOrEmpty(durum))
            {
                bool aktifMi = durum == "aktif";
                kullanicilar = kullanicilar.Where(k => k.Aktif == aktifMi);
            }
            
            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(arama))
            {
                kullanicilar = kullanicilar.Where(k => 
                    k.Ad.Contains(arama) || 
                    k.Soyad.Contains(arama) || 
                    k.Email.Contains(arama));
            }
            
            // Sıralama uygula
            kullanicilar = siralama switch
            {
                "ad_asc" => kullanicilar.OrderBy(k => k.Ad).ThenBy(k => k.Soyad),
                "ad_desc" => kullanicilar.OrderByDescending(k => k.Ad).ThenByDescending(k => k.Soyad),
                "email_asc" => kullanicilar.OrderBy(k => k.Email),
                "email_desc" => kullanicilar.OrderByDescending(k => k.Email),
                "rol_asc" => kullanicilar.OrderBy(k => k.Rol),
                "rol_desc" => kullanicilar.OrderByDescending(k => k.Rol),
                "tarih_asc" => kullanicilar.OrderBy(k => k.CreatedAt),
                "tarih_desc" => kullanicilar.OrderByDescending(k => k.CreatedAt),
                "id_desc" => kullanicilar.OrderByDescending(k => k.Id),
                _ => kullanicilar.OrderBy(k => k.Id), // Default: id_asc
            };
            
            await LoadKullaniciIstatistikleri();
            
            return View(await kullanicilar.ToListAsync());
        }

        // Kullanıcı düzenleme formu
        public async Task<IActionResult> KullaniciDuzenle(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
        }

        // Kullanıcı düzenleme POST işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciDuzenle(int id, Kullanici kullanici)
        {
            if (id != kullanici.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut kullanıcıyı getir
                    var existingKullanici = await _context.Kullanicilar.FindAsync(id);
                    if (existingKullanici == null)
                    {
                        return NotFound();
                    }

                    // Gerekli alanları güncelle
                    existingKullanici.Ad = kullanici.Ad;
                    existingKullanici.Soyad = kullanici.Soyad;
                    existingKullanici.Email = kullanici.Email;
                    existingKullanici.Rol = kullanici.Rol;
                    existingKullanici.Aktif = kullanici.Aktif;
                    existingKullanici.UpdatedAt = DateTime.Now;

                    _context.Update(existingKullanici);
                    await _context.SaveChangesAsync();

                    await GuncelleBagliKayitlar(existingKullanici);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KullaniciExists(kullanici.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Kullanicilar));
            }
            return View(kullanici);
        }

        // Kullanıcı silme onayı
        public async Task<IActionResult> KullaniciSil(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            return View(kullanici);
        }

        // Kullanıcı silme POST işlemi
        [HttpPost, ActionName("KullaniciSil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciSilOnay(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            bool hasRelatedRecords = await KullaniciHasRelatedRecords(kullanici);

            if (hasRelatedRecords)
            {
                // İlişkili kayıtlar varsa, aktifliği kaldır
                kullanici.Aktif = false;
                _context.Update(kullanici);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kullanıcının ilişkili kayıtları olduğundan pasif duruma alındı.";
            }
            else
            {
                // İlişkili kayıt yoksa, kaydı sil
                _context.Kullanicilar.Remove(kullanici);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kullanıcı başarıyla silindi.";
            }

            return RedirectToAction(nameof(Kullanicilar));
        }
        
        // Admin Dashboard - İstatistiksel grafikler ve raporlar
        public async Task<IActionResult> Dashboard()
        {
            // Kullanıcı istatistikleri
            await LoadKullaniciIstatistikleri();
            
            // Proje istatistikleri
            await LoadProjeIstatistikleri();
            
            // Akademisyen başına düşen proje sayısı (ilk 5)
            await LoadAkademisyenProjeSayilari();
            
            // Son eklenen projeler
            await LoadSonProjeler();
                
            // Son 6 ayın kullanıcı kayıt istatistikleri (grafik için)
            await LoadAylikKayitlar();
            
            return View();
        }
        
        // Kullanıcı Etkinlikleri Sayfası
        public async Task<IActionResult> KullaniciEtkinlikleri(int? kullaniciId = null, string tip = "", int sayfa = 1)
        {
            int sayfaBoyutu = 20;
            
            // Etkinlikleri temsil edecek model
            var etkinlikler = await GetEtkinlikListesi();
            
            // Filtreleme işlemleri
            if (kullaniciId.HasValue)
            {
                etkinlikler = etkinlikler.Where(e => e.KullaniciId == kullaniciId).ToList();
            }
            
            if (!string.IsNullOrEmpty(tip))
            {
                etkinlikler = etkinlikler.Where(e => e.Tip == tip).ToList();
            }
            
            // Tüm kullanıcı listesi (dropdown için)
            var kullaniciListesi = await _context.Kullanicilar
                .OrderBy(k => k.Ad)
                .ThenBy(k => k.Soyad)
                .Select(k => new SelectListItem { 
                    Value = k.Id.ToString(), 
                    Text = $"{k.Ad} {k.Soyad}",
                    Selected = kullaniciId.HasValue && k.Id == kullaniciId
                })
                .ToListAsync();
            
            ViewBag.Kullanicilar = kullaniciListesi;
            ViewBag.SecilenKullaniciId = kullaniciId;
            ViewBag.SecilenTip = tip;
                
            // Sayfalama işlemleri
            var toplamKayit = etkinlikler.Count;
            var toplamSayfa = (int)Math.Ceiling(toplamKayit / (double)sayfaBoyutu);
            
            etkinlikler = etkinlikler
                .OrderByDescending(e => e.Tarih)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .ToList();
                
            ViewBag.ToplamKayit = toplamKayit;
            ViewBag.ToplamSayfa = toplamSayfa;
            ViewBag.SuankiSayfa = sayfa;
            
            return View(etkinlikler);
        }

        #region Helper Methods
        
        private async Task LoadKullaniciIstatistikleri()
        {
            ViewBag.ToplamKullanici = await _context.Kullanicilar.CountAsync();
            ViewBag.AktifKullanici = await _context.Kullanicilar.CountAsync(k => k.Aktif);
            ViewBag.PasifKullanici = await _context.Kullanicilar.CountAsync(k => !k.Aktif);
            
            ViewBag.AdminSayisi = await _context.Kullanicilar.CountAsync(k => k.Rol == "Admin");
            ViewBag.AkademisyenSayisi = await _context.Kullanicilar.CountAsync(k => k.Rol == "Akademisyen");
            ViewBag.OgrenciSayisi = await _context.Kullanicilar.CountAsync(k => k.Rol == "Ogrenci");
            
            ViewBag.SonKullanicilar = await _context.Kullanicilar
                .OrderByDescending(k => k.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
        
        private async Task LoadProjeIstatistikleri()
        {
            ViewBag.ToplamProje = await _context.Projeler.CountAsync();
            ViewBag.BeklemedeProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Beklemede");
            ViewBag.DevamEdenProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Devam");
            ViewBag.TamamlananProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Tamamlandi");
            ViewBag.IptalProjeSayisi = await _context.Projeler.CountAsync(p => p.Status == "Iptal");
        }
        
        private async Task LoadAkademisyenProjeSayilari()
        {
            ViewBag.AkademisyenProjeSayilari = await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .Where(a => a.Kullanici.Aktif)
                .Select(a => new {
                    Akademisyen = a,
                    ProjeSayisi = _context.Projeler.Count(p => p.MentorId == a.Id)
                })
                .OrderByDescending(x => x.ProjeSayisi)
                .Take(5)
                .ToListAsync();
        }
        
        private async Task LoadSonProjeler()
        {
            ViewBag.SonProjeler = await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .OrderByDescending(p => p.OlusturmaTarihi)
                .Take(5)
                .ToListAsync();
        }
        
        private async Task LoadAylikKayitlar()
        {
            var aylikKayitVerileri = new List<KeyValuePair<string, int>>();
            
            for (int i = 5; i >= 0; i--)
            {
                var tarih = DateTime.Now.AddMonths(-i);
                var ay = tarih.Month;
                var yil = tarih.Year;
                
                var kayitSayisi = await _context.Kullanicilar
                    .CountAsync(k => k.CreatedAt.Month == ay && k.CreatedAt.Year == yil);
                
                var ayAdi = tarih.ToString("MMM yy", new System.Globalization.CultureInfo("tr-TR"));
                aylikKayitVerileri.Add(new KeyValuePair<string, int>(ayAdi, kayitSayisi));
            }
            
            ViewBag.AylikKayitlar = aylikKayitVerileri;
        }
        
        private async Task<bool> KullaniciHasRelatedRecords(Kullanici kullanici)
        {
            bool hasRelatedRecords = false;

            if (kullanici.Rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                if (akademisyen != null)
                {
                    hasRelatedRecords = await _context.Projeler.AnyAsync(p => p.MentorId == akademisyen.Id);
                }
            }
            else if (kullanici.Rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                if (ogrenci != null)
                {
                    hasRelatedRecords = await _context.Projeler.AnyAsync(p => p.OgrenciId == ogrenci.Id);
                }
            }
            
            return hasRelatedRecords;
        }
        
        private async Task GuncelleBagliKayitlar(Kullanici kullanici)
        {
            if (kullanici.Rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
                if (akademisyen != null)
                {
                    akademisyen.Ad = kullanici.Ad;
                    akademisyen.Soyad = kullanici.Soyad;
                    akademisyen.Email = kullanici.Email;
                    akademisyen.UpdatedAt = DateTime.Now;
                    _context.Update(akademisyen);
                    await _context.SaveChangesAsync();
                }
            }
            else if (kullanici.Rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
                if (ogrenci != null)
                {
                    ogrenci.Ad = kullanici.Ad;
                    ogrenci.Soyad = kullanici.Soyad;
                    ogrenci.Email = kullanici.Email;
                    ogrenci.UpdatedAt = DateTime.Now;
                    _context.Update(ogrenci);
                    await _context.SaveChangesAsync();
                }
            }
        }
        
        private async Task<List<dynamic>> GetEtkinlikListesi()
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
                .Take(50)
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
                .Take(50)
                .ToListAsync();
                
            etkinlikler.AddRange(projeler);
            
            return etkinlikler;
        }
        
        private bool KullaniciExists(int id)
        {
            return _context.Kullanicilar.Any(e => e.Id == id);
        }
        
        #endregion
    }
} 