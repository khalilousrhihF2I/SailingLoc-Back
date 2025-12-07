using Core.Entities;
using Core.Interfaces.PasswordReset;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Data.Repositories
{
    public class PasswordResetCodeRepository : IPasswordResetCodeRepository
    {
        private readonly ApplicationDbContext _db;
        public PasswordResetCodeRepository(ApplicationDbContext db) => _db = db;

        public async Task InvalidateActiveCodesAsync(Guid userId, string purpose, CancellationToken ct)
        {
            var q = _db.Set<PasswordResetCode>()
                .Where(x => x.UserId == userId && !x.Used && x.Purpose == purpose);
            await q.ExecuteDeleteAsync(ct);
        }

        public async Task AddAsync(PasswordResetCode code, CancellationToken ct)
        {
           
                await _db.AddAsync(code, ct);
                await _db.SaveChangesAsync(ct);


        }

        public Task<PasswordResetCode?> GetLatestActiveAsync(Guid userId, string purpose, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            return _db.Set<PasswordResetCode>()
                .Where(x => x.UserId == userId && !x.Used && x.Purpose == purpose && x.ExpiresAt > now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task IncrementAttemptsAsync(Guid id, CancellationToken ct)
        {
            var code = await _db.Set<PasswordResetCode>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (code is null) return;
            code.Attempts += 1;
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkUsedAsync(Guid id, CancellationToken ct)
        {
                var code = await _db.Set<PasswordResetCode>().FirstOrDefaultAsync(x => x.Id == id);
                if (code is null) return;
                code.Used = true;
                await _db.SaveChangesAsync(ct);          
        }
    }
}
