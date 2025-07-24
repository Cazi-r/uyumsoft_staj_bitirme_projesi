using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class ZamanDurumuService
    {
        private readonly ApplicationDbContext _context;
        
        public ZamanDurumuService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // Tüm görüşmelerin zaman durumlarını günceller
        public async Task UpdateAllZamanDurumuAsync()
        {
            var allMeetings = await _context.DanismanlikGorusmeleri
                .ToListAsync();
                
            foreach (var meeting in allMeetings)
            {
                meeting.GuncelleZamanDurumu();
            }
            
            await _context.SaveChangesAsync();
        }
        
        // Görüşme detayları sayfasına erişildiğinde kullanılabilir
        public async Task UpdateSingleZamanDurumuAsync(int gorusmeId)
        {
            var meeting = await _context.DanismanlikGorusmeleri
                .FindAsync(gorusmeId);
                
            if (meeting != null)
            {
                meeting.GuncelleZamanDurumu();
                await _context.SaveChangesAsync();
            }
        }
    }
} 