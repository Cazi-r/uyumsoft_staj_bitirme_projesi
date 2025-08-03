using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class BildirimService : IBildirimService
    {
        private readonly ApplicationDbContext _context;

        public BildirimService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Proje oluşturulduğunda bildirim gönder
        public async Task ProjeOlusturulduBildirimiGonder(Proje proje, string olusturanRol)
        {
            if (proje == null)
                return;

            if (olusturanRol == "Akademisyen" && proje.OgrenciId.HasValue)
            {
                // Akademisyen oluşturdu, öğrenciye bildirim
                await BildirimOlustur(
                    $"Yeni proje atandı: {proje.Ad}",
                    $"{await GetAkademisyenAdSoyad(proje.MentorId.Value)} size yeni bir proje atadı: {proje.Ad}",
                    "Bilgi",
                    ogrenciId: proje.OgrenciId.Value);
            }
            else if (olusturanRol == "Ogrenci" && proje.MentorId.HasValue)
            {
                // Öğrenci oluşturdu, akademisyene bildirim
                await BildirimOlustur(
                    $"Yeni proje önerisi: {proje.Ad}",
                    $"{await GetOgrenciAdSoyad(proje.OgrenciId.Value)} yeni bir proje önerdi: {proje.Ad}",
                    "Bilgi",
                    akademisyenId: proje.MentorId.Value);
            }
        }

        // 2. Proje yorumu yapıldığında bildirim gönder
        public async Task ProjeYorumYapildiBildirimiGonder(ProjeYorum yorum, int projeId)
        {
            if (yorum == null)
                return;

            var proje = await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje == null)
                return;

            string yorumSahibi = "";
            if (yorum.OgrenciId.HasValue)
            {
                yorumSahibi = await GetOgrenciAdSoyad(yorum.OgrenciId.Value);
                
                // Öğrenci yorum yaptı, akademisyene bildir
                if (proje.MentorId.HasValue)
                {
                    await BildirimOlustur(
                        $"Projeye yeni yorum: {proje.Ad}",
                        $"{yorumSahibi} '{proje.Ad}' projesine yorum yaptı: \"{yorum.Icerik.Substring(0, Math.Min(yorum.Icerik.Length, 50))}...\"",
                        "Bilgi",
                        akademisyenId: proje.MentorId.Value);
                }
            }
            else if (yorum.AkademisyenId.HasValue)
            {
                yorumSahibi = await GetAkademisyenAdSoyad(yorum.AkademisyenId.Value);
                
                // Akademisyen yorum yaptı, öğrenciye bildir
                if (proje.OgrenciId.HasValue)
                {
                    await BildirimOlustur(
                        $"Projeye yeni yorum: {proje.Ad}",
                        $"{yorumSahibi} '{proje.Ad}' projesine yorum yaptı: \"{yorum.Icerik.Substring(0, Math.Min(yorum.Icerik.Length, 50))}...\"",
                        "Bilgi",
                        ogrenciId: proje.OgrenciId.Value);
                }
            }
        }

        // 3. Değerlendirme yapıldığında bildirim gönder
        public async Task DegerlendirmeYapildiBildirimiGonder(Degerlendirme degerlendirme)
        {
            if (degerlendirme == null)
                return;

            var proje = await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .FirstOrDefaultAsync(p => p.Id == degerlendirme.ProjeId);

            if (proje == null || !proje.OgrenciId.HasValue)
                return;

            string akademisyenAd = await GetAkademisyenAdSoyad(degerlendirme.AkademisyenId);

            await BildirimOlustur(
                $"Projeniz değerlendirildi: {proje.Ad}",
                $"{akademisyenAd} '{proje.Ad}' projenizi değerlendirdi. Puan: {degerlendirme.Puan}/100",
                "Bilgi",
                ogrenciId: proje.OgrenciId.Value);
        }

        // 4. Proje ilerlemesi değiştiğinde bildirim gönder
        public async Task ProjeIlerlemesiDegistiBildirimiGonder(Proje proje)
        {
            if (proje == null || !proje.OgrenciId.HasValue)
                return;

            await BildirimOlustur(
                $"Proje durumu güncellendi: {proje.Ad}",
                $"'{proje.Ad}' projesinin durumu '{proje.Status}' olarak değiştirildi.",
                "Bilgi",
                ogrenciId: proje.OgrenciId.Value);
        }

        // 5. Danışmanlık görüşmesi planlandığında bildirim gönder
        public async Task GorusmePlanlandiBildirimiGonder(DanismanlikGorusmesi gorusme)
        {
            if (gorusme == null)
                return;

            var proje = await _context.Projeler
                .FirstOrDefaultAsync(p => p.Id == gorusme.ProjeId);

            if (proje == null)
                return;

            string akademisyenAd = await GetAkademisyenAdSoyad(gorusme.AkademisyenId);
            string ogrenciAd = await GetOgrenciAdSoyad(gorusme.OgrenciId);
            
            // Öğrenciye bildirim
            await BildirimOlustur(
                $"Danışmanlık görüşmesi planlandı",
                $"{akademisyenAd} ile {gorusme.GorusmeTarihi.ToString("dd.MM.yyyy HH:mm")} tarihinde {gorusme.GorusmeTipi} görüşmeniz var.",
                "Bilgi",
                ogrenciId: gorusme.OgrenciId);
            
            // Akademisyene bildirim
            await BildirimOlustur(
                $"Danışmanlık görüşmesi planlandı",
                $"{ogrenciAd} ile {gorusme.GorusmeTarihi.ToString("dd.MM.yyyy HH:mm")} tarihinde {gorusme.GorusmeTipi} görüşmeniz var.",
                "Bilgi",
                akademisyenId: gorusme.AkademisyenId);
        }

        // 6. Proje aşaması tamamlandığında bildirim gönder
        public async Task ProjeAsamasiTamamlandiBildirimiGonder(ProjeAsamasi projeAsamasi)
        {
            if (projeAsamasi == null)
                return;

            var proje = await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .FirstOrDefaultAsync(p => p.Id == projeAsamasi.ProjeId);

            if (proje == null || !proje.MentorId.HasValue)
                return;

            string ogrenciAd = await GetOgrenciAdSoyad(proje.OgrenciId.Value);

            await BildirimOlustur(
                $"Proje aşaması tamamlandı: {projeAsamasi.AsamaAdi}",
                $"{ogrenciAd} '{proje.Ad}' projesinde '{projeAsamasi.AsamaAdi}' aşamasını tamamladı. Değerlendirmeniz gerekiyor.",
                "Uyari",
                akademisyenId: proje.MentorId.Value);
        }

        // 7. Görüşme durumu değiştiğinde bildirim gönder
        public async Task GorusmeDurumuDegistiBildirimiGonder(DanismanlikGorusmesi gorusme)
        {
            if (gorusme == null)
                return;

            string guncelleyenAd = "";
            string aliciRol = "";
            int aliciId = 0;
            string mesaj = "";

            var akademisyen = await _context.Akademisyenler.FindAsync(gorusme.AkademisyenId);
            var ogrenci = await _context.Ogrenciler.FindAsync(gorusme.OgrenciId);

            if (akademisyen == null || ogrenci == null)
                return;

            if (gorusme.SonGuncelleyenRol == "Ogrenci")
            {
                guncelleyenAd = $"{ogrenci.Ad} {ogrenci.Soyad}";
                aliciRol = "Akademisyen";
                aliciId = gorusme.AkademisyenId;
            }
            else
            {
                guncelleyenAd = $"{akademisyen.Unvan} {akademisyen.Ad} {akademisyen.Soyad}";
                aliciRol = "Ogrenci";
                aliciId = gorusme.OgrenciId;
            }

            switch (gorusme.Durum)
            {
                case GorusmeDurumu.Onaylandi:
                    mesaj = $"{guncelleyenAd}, '{gorusme.Baslik}' başlıklı görüşme talebinizi onayladı.";
                    break;
                case GorusmeDurumu.IptalEdildi:
                    mesaj = $"{guncelleyenAd}, '{gorusme.Baslik}' başlıklı görüşme talebinizi iptal etti/reddetti.";
                    break;
                case GorusmeDurumu.OgrenciOnayiBekliyor:
                case GorusmeDurumu.HocaOnayiBekliyor:
                    mesaj = $"{guncelleyenAd}, '{gorusme.Baslik}' başlıklı görüşme için yeni bir tarih önerdi: {gorusme.GorusmeTarihi:dd.MM.yyyy HH:mm}. Lütfen kontrol ediniz.";
                    break;
            }

            if (!string.IsNullOrEmpty(mesaj))
            {
                if (aliciRol == "Ogrenci")
                {
                    await BildirimOlustur("Görüşme Durumu Güncellendi", mesaj, "Bilgi", ogrenciId: aliciId);
                }
                else
                {
                    await BildirimOlustur("Görüşme Durumu Güncellendi", mesaj, "Bilgi", akademisyenId: aliciId);
                }
            }
        }

        // Okunmamış bildirim sayısını getir
        public async Task<int> OkunmamisBildirimSayisiniGetir(string kullaniciId, string rol)
        {
            if (string.IsNullOrEmpty(kullaniciId) || string.IsNullOrEmpty(rol))
                return 0;

            if (rol == "Ogrenci")
            {
                var ogrenci = await _context.Ogrenciler.FirstOrDefaultAsync(o => o.KullaniciId.ToString() == kullaniciId);
                if (ogrenci != null)
                {
                    return await _context.Bildirimler
                        .CountAsync(b => b.OgrenciId == ogrenci.Id && !b.Okundu);
                }
            }
            else if (rol == "Akademisyen")
            {
                var akademisyen = await _context.Akademisyenler.FirstOrDefaultAsync(a => a.KullaniciId.ToString() == kullaniciId);
                if (akademisyen != null)
                {
                    return await _context.Bildirimler
                        .CountAsync(b => b.AkademisyenId == akademisyen.Id && !b.Okundu);
                }
            }

            return 0;
        }

        // Yardımcı metotlar
        private async Task BildirimOlustur(string baslik, string icerik, string bildirimTipi, int? ogrenciId = null, int? akademisyenId = null)
        {
            var bildirim = new Bildirim
            {
                Baslik = baslik,
                Icerik = icerik,
                OlusturmaTarihi = DateTime.Now,
                BildirimTipi = bildirimTipi,
                OgrenciId = ogrenciId,
                AkademisyenId = akademisyenId,
                Okundu = false
            };

            _context.Bildirimler.Add(bildirim);
            await _context.SaveChangesAsync();
        }

        private async Task<string> GetOgrenciAdSoyad(int ogrenciId)
        {
            var ogrenci = await _context.Ogrenciler.FindAsync(ogrenciId);
            return ogrenci != null ? $"{ogrenci.Ad} {ogrenci.Soyad}" : "Öğrenci";
        }

        private async Task<string> GetAkademisyenAdSoyad(int akademisyenId)
        {
            var akademisyen = await _context.Akademisyenler.FindAsync(akademisyenId);
            return akademisyen != null ? $"{akademisyen.Unvan} {akademisyen.Ad} {akademisyen.Soyad}" : "Akademisyen";
        }
    }
} 