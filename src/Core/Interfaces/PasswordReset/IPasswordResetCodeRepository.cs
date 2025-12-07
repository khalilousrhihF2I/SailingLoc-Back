using Core.Entities;

namespace Core.Interfaces.PasswordReset
{
    public interface IPasswordResetCodeRepository
    {
        Task InvalidateActiveCodesAsync(Guid userId, string purpose, CancellationToken ct);
        Task AddAsync(PasswordResetCode code, CancellationToken ct);
        Task<PasswordResetCode?> GetLatestActiveAsync(Guid userId, string purpose, CancellationToken ct);
        Task IncrementAttemptsAsync(Guid id, CancellationToken ct);
        Task MarkUsedAsync(Guid id, CancellationToken ct);
    }
}
