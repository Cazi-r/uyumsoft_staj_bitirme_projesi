using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class AkademisyenService : IAkademisyenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Akademisyen> _akademisyenRepository;
        private readonly IProjeService _projeService;

        public AkademisyenService(
            ApplicationDbContext context,
            IRepository<Akademisyen> akademisyenRepository,
            IProjeService projeService)
        {
            _context = context;
            _akademisyenRepository = akademisyenRepository;
            _projeService = projeService;
        }

        /// Tum akademisyenleri iliskili Kullanici bilgisiyle birlikte listeler.
        public async Task<IEnumerable<Akademisyen>> GetAllAsync()
        {
            return await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .ToListAsync();
        }

        /// Id'ye gore akademisyeni iliskili Kullanici bilgisiyle birlikte dondurur.
        public async Task<Akademisyen> GetByIdAsync(int id)
        {
            return await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// KullaniciId'ye gore akademisyeni dondurur.
        public async Task<Akademisyen> GetByKullaniciIdAsync(int kullaniciId)
        {
            return await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.KullaniciId == kullaniciId);
        }

        /// Kullanici e-postasina gore bagli akademisyeni dondurur.
        public async Task<Akademisyen> GetAkademisyenByUserName(string userName)
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email == userName);
            
            if (kullanici == null)
                return null;
            
            return await _context.Akademisyenler
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.KullaniciId == kullanici.Id);
        }

        /// Yeni akademisyen kaydi olusturur.
        public async Task<Akademisyen> AddAsync(Akademisyen akademisyen)
        {
            await _akademisyenRepository.AddAsync(akademisyen);
            return akademisyen;
        }

        /// Akademisyen kaydini gunceller.
        public async Task UpdateAsync(Akademisyen akademisyen)
        {
            await _akademisyenRepository.UpdateAsync(akademisyen);
        }

        /// Id'ye gore akademisyen kaydini siler.
        public async Task DeleteAsync(int id)
        {
            await _akademisyenRepository.DeleteAsync(id);
        }

        /// Akademisyenin danismanlik ettigi projeleri dondurur.
        public async Task<IEnumerable<Proje>> GetProjelerAsync(int akademisyenId)
        {
            return await _projeService.GetByMentorIdAsync(akademisyenId);
        }

        /// Akademisyenin yaptigi degerlendirmeleri ilgili projeleriyle birlikte dondurur.
        public async Task<IEnumerable<Degerlendirme>> GetDegerlendirmelerAsync(int akademisyenId)
        {
            return await _context.Degerlendirmeler
                .Include(d => d.Proje)
                .Where(d => d.AkademisyenId == akademisyenId)
                .ToListAsync();
        }
    }
} 