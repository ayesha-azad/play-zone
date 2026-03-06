using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlayZone.BLL.Interfaces;
using PlayZone.DAL.Data.Contexts;
using PlayZone.DAL.Models;

namespace PlayZone.BLL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly PlayZoneDbContext _dbContext;

        public GenericRepository(PlayZoneDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<ICollection<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Game))
            {
                return (ICollection<T>)await _dbContext.Games
                    .Include(G => G.Category)
                    .Include(g => g.Device)
                    .ThenInclude(gd => gd.Device)
                    .ToListAsync();
            }
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            if (typeof(T) == typeof(Game))
            {
                var game = await _dbContext.Games
                    .Include(g => g.Category)
                    .Include(g => g.Device)
                        .ThenInclude(gd => gd.Device)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(g => g.Id == id);

                return game as T;
            }
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public async Task CreateAsync(T entity)
        {
            if (entity is not null)
            {
                await _dbContext.Set<T>().AddAsync(entity);
            }
        }

        public void UpdateAsync(T entity)
        {
            if (entity is not null)
            {
                _dbContext.Set<T>().Update(entity);
            }
        }

        public void DeleteAsync(Guid id)
        {
            var entity = _dbContext.Set<T>().Find(id);
            if (entity is not null)
            {
                _dbContext.Set<T>().Remove(entity);
            }
        }
    }
}
