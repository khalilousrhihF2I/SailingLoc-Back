namespace Core.Entities;
public class PasswordResetToken {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Token { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public bool Used { get; set; }
  public Guid UserId { get; set; }
  public AppUser User { get; set; } = null!;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
