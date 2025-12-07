using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) { _db = db; }

        public IQueryable<AppUser> Query() => _db.Users.AsQueryable();

        public Task<AppUser?> GetUserWithDetailsAsync(Guid id)
        {
            return _db.Users
                .Include(u => u.UserDocuments)
                .Include(u => u.BoatsOwned).ThenInclude(b => b.Bookings)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<bool> UserExistsAsync(Guid id)
        {
            return _db.Users.AnyAsync(u => u.Id == id);
        }
    }
}
