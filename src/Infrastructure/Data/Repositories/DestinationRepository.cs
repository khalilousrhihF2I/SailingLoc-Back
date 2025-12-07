using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class DestinationRepository
    {
        private readonly ApplicationDbContext _db;

        public DestinationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Destination> Query()
        {
            return _db.Destinations.AsQueryable();
        }

        public Task<Destination?> GetByIdAsync(int id)
        {
            return _db.Destinations.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Destination dest)
        {
            await _db.Destinations.AddAsync(dest);
        }

        public void Remove(Destination dest)
        {
            _db.Destinations.Remove(dest);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
