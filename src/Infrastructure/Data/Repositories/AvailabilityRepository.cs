using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class AvailabilityRepository
    {
        private readonly ApplicationDbContext _db;
        public AvailabilityRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<BoatAvailability> Query()
        {
            return _db.BoatAvailabilities.AsQueryable();
        }

        public Task<BoatAvailability?> GetByIdAsync(int id)
        {
            return _db.BoatAvailabilities.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(BoatAvailability a)
        {
            await _db.BoatAvailabilities.AddAsync(a);
        }

        public void Remove(BoatAvailability a)
        {
            _db.BoatAvailabilities.Remove(a);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
