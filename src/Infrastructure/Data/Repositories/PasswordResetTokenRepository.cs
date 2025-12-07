using Core.Entities;
using Core.Interfaces.PasswordReset;

namespace Infrastructure.Data.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _db;
        public PasswordResetTokenRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(PasswordResetToken token, CancellationToken ct)
        {
            _db.PasswordResetTokens.Add(token);
            await _db.SaveChangesAsync(ct);
        }
    }
}
