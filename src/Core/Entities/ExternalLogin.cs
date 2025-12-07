namespace Core.Entities;
public class ExternalLogin {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Provider { get; set; } = string.Empty;
  public string ProviderKey { get; set; } = string.Empty;
  public Guid UserId { get; set; }
  public AppUser User { get; set; } = null!;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
