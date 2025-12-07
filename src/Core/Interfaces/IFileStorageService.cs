namespace Core.Interfaces;
public interface IFileStorageService {
  Task<string> SaveAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
}
