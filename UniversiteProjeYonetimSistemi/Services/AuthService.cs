using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;
using System.Security.Cryptography;
using System.Text;
using UniversiteProjeYonetimSistemi.Models.ViewModels;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// Kullanici email ve sifre ile giris yapar; rol ve diger claim'leri olusturarak cookie tabanli oturumu baslatir.
        /// Basarili olursa true, aksi halde false dondurur.
        public async Task<bool> LoginAsync(string email, string password)
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(u => u.Email == email && u.Aktif);

            if (kullanici != null && VerifyPassword(password, kullanici.Sifre))
            {
                // Oturum oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, kullanici.Email),
                    new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                    new Claim(ClaimTypes.Role, kullanici.Rol),
                    new Claim("Ad", kullanici.Ad),
                    new Claim("Soyad", kullanici.Soyad)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Son giriş tarihini güncelle
                kullanici.SonGiris = DateTime.Now;
                _context.Update(kullanici);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// Mevcut kullanicinin oturumunu sonlandirir ve ilgili cookie/claim bilgilerini temizler.
        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// HttpContext uzerindeki claim'lerden kullanici kimligini okuyup veritabanindan mevcut Kullanici kaydini dondurur.
        /// Kimlik bulunamazsa null dondurur.
        public async Task<Kullanici> GetCurrentUserAsync()
        {
            if (_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated != true)
                return null;

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                return null;

            return await _context.Kullanicilar.FindAsync(id);
        }

        /// Aktif kullanici Ogrenci rolundeyse bagli Ogrenci kaydini dondurur; degilse null dondurur.
        public async Task<Ogrenci> GetCurrentOgrenciAsync()
        {
            var kullanici = await GetCurrentUserAsync();
            if (kullanici == null || kullanici.Rol != "Ogrenci")
                return null;

            return await _context.Ogrenciler
                .FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
        }

        /// Aktif kullanici Akademisyen rolundeyse bagli Akademisyen kaydini dondurur; degilse null dondurur.
        public async Task<Akademisyen> GetCurrentAkademisyenAsync()
        {
            var kullanici = await GetCurrentUserAsync();
            if (kullanici == null || kullanici.Rol != "Akademisyen")
                return null;

            return await _context.Akademisyenler
                .FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
        }

        /// Verilen Id'ye ait Ogrenci kaydini iliskili Kullanici bilgisi ile birlikte getirir.
        public async Task<Ogrenci> GetOgrenciByIdAsync(int id)
        {
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// Verilen Id'ye ait Akademisyen kaydini iliskili Kullanici bilgisi ile birlikte getirir.
        public async Task<Akademisyen> GetAkademisyenByIdAsync(int id)
        {
            return await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Yeni kayıt metodu - Admin rolü için düzeltildi
        /// Yeni kullanici kaydi olusturur; role gore Ogrenci/Akademisyen tablolarina bagli kaydi ekler.
        /// Islem, transaction ve execution strategy ile guvenli sekilde yapilir.
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            // E-posta adresinin benzersiz olduğunu kontrol et
            if (await _context.Kullanicilar.AnyAsync(u => u.Email == model.Email))
            {
                return (false, "Bu e-posta adresi zaten kullanılıyor.");
            }

            // Öğrenci numarası kontrolü
            if (model.Rol == "Ogrenci")
            {
                if (string.IsNullOrEmpty(model.OgrenciNo))
                {
                    return (false, "Öğrenci numarası zorunludur.");
                }
                
                if (await _context.Ogrenciler.AnyAsync(o => o.OgrenciNo == model.OgrenciNo))
                {
                    return (false, "Bu öğrenci numarası zaten kayıtlı.");
                }
            }
            
            // Akademisyen unvan kontrolü
            if (model.Rol == "Akademisyen" && string.IsNullOrEmpty(model.Unvan))
            {
                return (false, "Unvan alanı zorunludur.");
            }

            // Execution Strategy kullanarak işlemleri retry-safe şekilde gerçekleştir
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            
            return await executionStrategy.ExecuteAsync(async () => {
                // Transaction başlat
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Kullanıcı oluştur
                    var kullanici = new Kullanici
                    {
                        Ad = model.Ad,
                        Soyad = model.Soyad,
                        Email = model.Email,
                        Sifre = HashPassword(model.Password),
                        Rol = model.Rol,
                        Aktif = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Kullanicilar.Add(kullanici);
                    await _context.SaveChangesAsync();

                    // Role göre öğrenci veya akademisyen kaydı oluştur
                    if (model.Rol == "Ogrenci")
                    {
                        var ogrenci = new Ogrenci
                        {
                            KullaniciId = kullanici.Id,
                            Ad = model.Ad,
                            Soyad = model.Soyad,
                            Email = model.Email,
                            OgrenciNo = model.OgrenciNo,
                            Telefon = !string.IsNullOrEmpty(model.Telefon) ? model.Telefon : string.Empty,
                            Adres = !string.IsNullOrEmpty(model.Adres) ? model.Adres : string.Empty,
                            KayitTarihi = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Ogrenciler.Add(ogrenci);
                    }
                    else if (model.Rol == "Akademisyen")
                    {
                        var akademisyen = new Akademisyen
                        {
                            KullaniciId = kullanici.Id,
                            Ad = model.Ad,
                            Soyad = model.Soyad,
                            Email = model.Email,
                            Unvan = !string.IsNullOrEmpty(model.Unvan) ? model.Unvan : "Belirtilmemiş",
                            UzmanlikAlani = model.UzmanlikAlani ?? string.Empty,
                            Ofis = !string.IsNullOrEmpty(model.Ofis) ? model.Ofis : "Belirtilmemiş",
                            Telefon = !string.IsNullOrEmpty(model.Telefon) ? model.Telefon : string.Empty,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Akademisyenler.Add(akademisyen);
                    }
                    // Admin için sadece kullanıcı tablosuna ekleme yapılıyor, özel tablo yok

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Kayıt başarıyla oluşturuldu.");
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    
                    // İç hata mesajını al
                    string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    
                    // Duplicate key hatası (SQL Server)
                    if (errorMessage.Contains("IX_") || errorMessage.Contains("UNIQUE KEY") || errorMessage.Contains("UNIQUE CONSTRAINT"))
                    {
                        return (false, "Bu e-posta adresi veya öğrenci numarası zaten kullanılmakta.");
                    }
                    
                    // Foreign key hatası
                    if (errorMessage.Contains("FOREIGN KEY"))
                    {
                        return (false, "İlişkili bir kayıt bulunamadı.");
                    }
                    
                    // Genel veritabanı hatası
                    return (false, $"Veritabanı hatası: {errorMessage}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    
                    // İç hataları da kontrol et
                    if (ex.InnerException != null)
                    {
                        return (false, $"Kayıt işlemi sırasında bir hata oluştu: {ex.Message}. İç hata: {ex.InnerException.Message}");
                    }
                    
                    return (false, $"Kayıt işlemi sırasında bir hata oluştu: {ex.Message}");
                }
            });
        }

        // Bu örnek için basit bir şifre doğrulama - gerçek uygulamada daha güvenli yöntemler kullanılmalı
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Bu basit bir karşılaştırmadır, gerçek uygulamada BCrypt veya PBKDF2 gibi daha güvenli bir yöntem kullanılmalı
            return ComputeSha256Hash(password) == hashedPassword;
        }

        // Şifre hash'leme (örnek amaçlı, gerçek uygulamalar için daha güvenli yöntemler kullanılmalı)
        /// Verilen sifreyi SHA-256 ile hash'ler. Ornek amaclidir; uretimde PBKDF2/BCrypt/Argon2 gibi yavas hash onerilir.
        public string HashPassword(string password)
        {
            return ComputeSha256Hash(password);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 