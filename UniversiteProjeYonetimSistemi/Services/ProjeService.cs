using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class ProjeService : IProjeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Proje> _projeRepository;

        public ProjeService(ApplicationDbContext context, IRepository<Proje> projeRepository)
        {
            _context = context;
            _projeRepository = projeRepository;
        }

        public async Task<IEnumerable<Proje>> GetAllAsync()
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .ToListAsync();
        }

        public async Task<Proje> GetByIdAsync(int id)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Include(p => p.Dosyalar)
                .Include(p => p.Yorumlar)
                .Include(p => p.Degerlendirmeler)
                .Include(p => p.Asamalar)
                .Include(p => p.Kaynaklar)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Proje>> GetByOgrenciIdAsync(int ogrenciId)
        {
            return await _context.Projeler
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Where(p => p.OgrenciId == ogrenciId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Proje>> GetByMentorIdAsync(int mentorId)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Kategori)
                .Where(p => p.MentorId == mentorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Proje>> GetByKategoriIdAsync(int kategoriId)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Where(p => p.KategoriId == kategoriId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Proje>> GetByStatusAsync(string status)
        {
            return await _context.Projeler
                .Include(p => p.Ogrenci)
                .Include(p => p.Mentor)
                .Include(p => p.Kategori)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<Proje> AddAsync(Proje proje)
        {
            proje.OlusturmaTarihi = DateTime.Now;
            await _projeRepository.AddAsync(proje);
            return proje;
        }

        public async Task UpdateAsync(Proje proje)
        {
            await _projeRepository.UpdateAsync(proje);
        }

        public async Task DeleteAsync(int id)
        {
            await _projeRepository.DeleteAsync(id);
        }

        public async Task AssignToOgrenciAsync(int projeId, int ogrenciId)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.OgrenciId = ogrenciId;
            proje.Status = "Atanmis";
            await _projeRepository.UpdateAsync(proje);
        }

        public async Task AssignToMentorAsync(int projeId, int mentorId)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.MentorId = mentorId;
            await _projeRepository.UpdateAsync(proje);
        }

        public async Task UpdateStatusAsync(int projeId, string newStatus)
        {
            var proje = await _projeRepository.GetByIdAsync(projeId);
            proje.Status = newStatus;
            await _projeRepository.UpdateAsync(proje);
        }
    }
} 