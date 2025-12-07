namespace Core.Entities;
public class PasswordResetCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    public string CodeHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }
    public int Attempts { get; set; }
    public string Purpose { get; set; } = "password-reset";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
