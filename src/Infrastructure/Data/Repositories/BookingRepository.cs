using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class BookingRepository
    {
        private readonly ApplicationDbContext _db;
        public BookingRepository(ApplicationDbContext db) { _db = db; }

        public IQueryable<Booking> Query()
        {
            return _db.Bookings.AsQueryable();
        }

        public Task<Booking?> GetByIdAsync(string id)
        {
            return _db.Bookings
                         .Include(b => b.Boat)
                             .ThenInclude(boat => boat.Owner)
                         .Include(b => b.Boat)
                             .ThenInclude(b => b.Images)
                         .Include(b => b.Renter)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking b)
        {
            await _db.Bookings.AddAsync(b);
        }

        public void Update(Booking b)
        {
            _db.Bookings.Update(b);
        }

        public void Remove(Booking b)
        {
            _db.Bookings.Remove(b);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
