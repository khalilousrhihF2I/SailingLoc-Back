using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class ReviewRepository
    {
        private readonly ApplicationDbContext _db;
        public ReviewRepository(ApplicationDbContext db) { _db = db; }

        public IQueryable<Review> Query()
        {
            return _db.Reviews.AsQueryable();
        }

        public Task<Review?> GetByIdAsync(int id)
        {
            return _db.Reviews.Include(r => r.User).Include(r => r.Boat).AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(Review r) => await _db.Reviews.AddAsync(r);
        public void Remove(Review r) => _db.Reviews.Remove(r);
        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
