using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class OgrenciService : IOgrenciService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Ogrenci> _ogrenciRepository;
        private readonly IProjeService _projeService;

        public OgrenciService(
            ApplicationDbContext context, 
            IRepository<Ogrenci> ogrenciRepository,
            IProjeService projeService)
        {
            _context = context;
            _ogrenciRepository = ogrenciRepository;
            _projeService = projeService;
        }

        public async Task<IEnumerable<Ogrenci>> GetAllAsync()
        {
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .ToListAsync();
        }

        public async Task<Ogrenci> GetByIdAsync(int id)
        {
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Ogrenci> GetByKullaniciIdAsync(int kullaniciId)
        {
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.KullaniciId == kullaniciId);
        }

        public async Task<Ogrenci> GetByOgrenciNoAsync(string ogrenciNo)
        {
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);
        }

        public async Task<Ogrenci> GetOgrenciByUserName(string userName)
        {
            // Direkt olarak _context kullanarak arama yapalım
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email == userName);
            
            if (kullanici == null)
                return null;
            
            return await _context.Ogrenciler
                .Include(o => o.Kullanici)
                .FirstOrDefaultAsync(o => o.KullaniciId == kullanici.Id);
        }

        public async Task<Ogrenci> AddAsync(Ogrenci ogrenci)
        {
            await _ogrenciRepository.AddAsync(ogrenci);
            return ogrenci;
        }

        public async Task UpdateAsync(Ogrenci ogrenci)
        {
            await _ogrenciRepository.UpdateAsync(ogrenci);
        }

        public async Task DeleteAsync(int id)
        {
            await _ogrenciRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Proje>> GetProjelerAsync(int ogrenciId)
        {
            return await _projeService.GetByOgrenciIdAsync(ogrenciId);
        }
    }
} 