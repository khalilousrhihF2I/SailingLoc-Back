namespace Core.Entities;
public class Profile {
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  public AppUser User { get; set; } = null!;
  public string? Bio { get; set; }
}
