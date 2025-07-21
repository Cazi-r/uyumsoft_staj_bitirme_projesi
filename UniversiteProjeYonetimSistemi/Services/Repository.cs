using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversiteProjeYonetimSistemi.Data;
using UniversiteProjeYonetimSistemi.Models;

namespace UniversiteProjeYonetimSistemi.Services
{
    public class Repository<T> : IRepository<T> where T : TemelVarlik
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        
        public async Task<T> GetByIdWithIncludeAsync(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.Now;
            
            // Mevcut entity'yi getir - böylece aynı ID'ye sahip iki entity örneği sorununu önlemiş oluruz
            var existingEntity = await _dbSet.FindAsync(entity.Id);
            
            if (existingEntity == null)
            {
                throw new Exception($"ID {entity.Id} olan entity bulunamadı.");
            }
            
            // Entity Framework'ün entry metodunu kullanarak sadece değiştirilmiş özellikleri güncelle
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            
            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(T entity)
        {
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
} 