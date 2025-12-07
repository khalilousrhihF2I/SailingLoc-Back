using Core.Entities;

namespace Core.Interfaces.PasswordReset
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token, CancellationToken ct);
    }
}
