using Core.Entities;

namespace Core.Interfaces.PasswordReset;

public interface IPasswordResetService
{

    Task SendResetCodeAsync(string email, string? phoneNumber, string channel, CancellationToken ct);
    Task<string> VerifyResetCodeAndIssueTokenAsync(string email, string code, CancellationToken ct);
    Task<string> CreatePasswordResetTokenAsync(AppUser user, CancellationToken cancellationToken = default);
}