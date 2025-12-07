using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    // Repository léger pour centraliser l'accès EF Core.
    public class BoatRepository
    {
        private readonly ApplicationDbContext _db;

        public BoatRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Boat> Query()
        {
            return _db.Boats
                .Include(b => b.Images)
                .Include(b => b.Availabilities)
                .Include(b => b.Reviews)
                .Include(b => b.Owner) 
                .AsTracking();                
        }

        public Task<Boat?> GetByIdAsync(int id)
        {
            return _db.Boats
                .Include(b => b.Images)
                .Include(b => b.Availabilities)
                .Include(b => b.Reviews)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Boat boat)
        {
            await _db.Boats.AddAsync(boat);
        }

        public void Remove(Boat boat)
        {
            _db.Boats.Remove(boat);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}